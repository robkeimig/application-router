# application-router
.NET Core Application Routing Framework

Utilizes:
- `AspNetCore`/`Kestrel` for servicing external requests.
- `HttpClient` (per managed request thread id) for making internal requests.

Features:
- [ ] Support all HTTP Verbs (very buggy GET-only sample here)
- [ ] TLS 
- [ ] ???

Crude performance validation:
![image](https://user-images.githubusercontent.com/13019172/66617463-cf7bd180-eb9a-11e9-8238-f25f95d34cf3.png)

How to use:
See TestAppFrontEnd for an idea of how this works (or doesn't).
