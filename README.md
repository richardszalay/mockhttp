[![Build status](https://ci.appveyor.com/api/projects/status/3in8hmcyg11wpcjw)](https://ci.appveyor.com/project/richardszalay/mockhttp)

MockHttp for HttpClient
=====================

MockHttp is a testing layer for Microsoft's HttpClient library. It allows stubbed responses to be configured for matched HTTP requests and can be used to test your application's service layer.

## NuGet

    PM> Install-Package RichardSzalay.MockHttp

## How?

MockHttp defines a replacement `HttpMessageHandler`, the engine that drives HttpClient, that provides a fluent configuration API and provides a canned response. The caller (eg. your application's service layer) remains unaware of its presence.

## Usage

```csharp
var mockHttp = new MockHttpMessageHandler();

// Setup a respond for the user api (including a wildcard in the URL)
mockHttp.When("http://localost/api/user/*")
        .Respond("application/json", "{'name' : 'Test McGee'}"); // Respond with JSON

// Inject the handler or client into your application code
var client = new HttpClient(mockHttp);

var response = async client.GetAsync("http://localost/api/user/1234");
// or without async: var response = client.GetAsync("http://localost/api/user/1234").Result;

var json = await response.Content.ReadAsStringAsync();

// No network connection required
Console.Write(json); // {'name' : 'Test McGee'}
```

### When (Backend Definitions) vs Expect (Request Expectations)

`MockHttpMessageHandler` defines both Both `When` and `Expect`, which can be used to define responses. They both expose the same fluent API, but each works in a slightly different way.

Using `When` specifies a "Backend Definition". Backend Definitions can be matched against multiple times and in any order, but they won't match if there are any outstanding Request Expectations present. If no Request Expectations match, `Fallback` will be used.

Using `Expect` specifies a "Request Expectation". Request Expectations match only once and in the order they were added in. Only once all expectations have been satisfied will Backend Definitions be evaluated. Calling `mockHttp.VerifyNoOutstandingExpectation()` will assert that there are no expectations that have yet to be called. Calling `ResetExpectations` clears the the queue of expectations.

This pattern is heavily inspired by [AngularJS's $httpBackend](https://docs.angularjs.org/api/ngMock/service/$httpBackend)

### Matchers (With*)

The `With` and `Expect` methods return a `MockedRequest`, which can have additional constraints (called matchers) placed on them before specifying a response with `Respond`.

Passing an HTTP method and URL to `When` or `Expect` is equivalent to applying a Method and Url matcher respectively. The following chart breaks down additional built in matchers and their usage:

| Method | Description |
| ------ | ----------- |
| <pre>WithQueryString("key", "value")<br /><br />WithQueryString("key=value&other=value")<br /><br />WithQueryString(new Dictionary&lt;string,string><br />{<br />  { "key", "value" },<br />  { "other", "value" }<br />}<br /></pre> | Matches on one or more querystring values |
| <pre>WithFormData("key", "value")<br /><br />WithFormData("key=value&other=value")<br /><br />WithFormData(new Dictionary&lt;string,string><br />{<br />  { "key", "value" },<br />  { "other", "value" }<br />})<br /></pre> | Matches on one or more form data values |
| <pre>WithContent("{'name':'McGee'}")</pre> | Matches on the (post) content of the request |
| <pre>WithPartialContent("McGee")</pre> | Matches on the partial (post) content of the request |
| <pre>WithHeaders("Authorization", "Basic abcdef")<br /><br />WithHeaders(@"Authorization: Basic abcdef<br />Accept: application/json")<br /><br />WithHeaders(new Dictionary&lt;string,string><br />{<br />  { "Authorization", "Basic abcdef" },<br />  { "Accept", "application/json" }<br />})<br /></pre> | Matches on one or more HTTP header values |
| <pre>With(request => request.Content.Length > 50)</pre> | Applies custom matcher logic against an HttpRequestMessage |

These methods are chainable, making complex requirements easy to descirbe.

### Examples

This example uses Expect to test an OAuth ticket recycle process:

```csharp
// Simulate an expired token
mockHttp.Expect("/users/me")
        .WithQueryString("access_token", "old_token")
        .Respond(HttpStatusCode.Unauthorized);
    
// Expect the request to refresh the token and supply a new one
mockHttp.Expect("/tokens/refresh")
        .WithFormData("refresh_token", "refresh_token")
        .Respond("application/json", "{'access_token' : 'new_token', 'refresh_token' : 'new_refresh'}");
    
// Expect the original call to be retried with the new token
mockHttp.Expect("/users/me")
        .WithQueryString("access_token", "new_token")
        .Respond("application/json", "{'name' : 'Test McGee'}");
    
var httpClient = new HttpClient(mockHttp);

var userService = new UserService(httpClient);

var user = await userService.GetUserDetails();

Assert.Equals("Test McGee", user.Name);
mockHttp.VerifyNoOutstandingExpectation();
```
	
## Platform Support

MockHttp is compiled for .NET Standard 1.1 (including .NET Core), .NET 4, and .NET 4.5, as well a Portable Class Library (Profile 328) supporting:

* .NET 4
* Silverlight 5
* Winodws 8
* Windows Phone Silverlight 8
* Windows Phone 8.1
* Xamarin iOS
* Xamarin Android

## Build / Release

Clone the repository and build `RichardSzalay.MockHttp.sln` using MSBuild. NuGet package restore must be enabled.

To release, build:

```
msbuild Release.proj /p:PackageVersion=1.2.3
```

If you fork the project, simply rename the `nuspec` file accordingly and it will be picked up by the release script.

## License

The MIT License (MIT)

Copyright (c) 2014 Richard Szalay

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
