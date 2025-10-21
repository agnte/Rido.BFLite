info: Rido.BFLite.Core.BotApplication[0]
      Started bot listener on http://localhost:3978 for AppID:9d17cc32-c91b-4368-8494-1b29ccb0dbcf
info: Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager[63]
      User profile is available. Using 'C:\Users\rmpablos\AppData\Local\ASP.NET\DataProtection-Keys' as key repository and Windows DPAPI to encrypt keys at rest.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:3978
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\_code\Rido.BFLite\samples\Samples.BotEcho
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://localhost:3978/api/messages - application/json;+charset=utf-8 1322
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      Microsoft.IdentityModel Version: 8.14.0.0. Date 10/21/2025 01:44:36. PII logging is OFF. See https://aka.ms/IdentityModel/PII for details.
      IDX10242: Security token: '[PII of type 'Microsoft.IdentityModel.JsonWebTokens.JsonWebToken' is hidden. For more details, see https://aka.ms/IdentityModel/PII.]' has a valid signature.
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10250: The associated certificate is valid. ValidFrom (UTC): '10/1/2025 5:16:36 AM', Current time (UTC): '10/21/2025 1:44:36 AM'.
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10251: The associated certificate is valid. ValidTo (UTC): '10/1/2030 5:16:36 AM', Current time (UTC): '10/21/2025 1:44:36 AM'.
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10239: Lifetime of the token is valid.
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10234: Audience Validated.Audience: '9d17cc32-c91b-4368-8494-1b29ccb0dbcf'
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10236: Issuer Validated.Issuer: 'https://login.microsoftonline.com/5369a35c-46a5-4677-8ff9-2e65587654e7/v2.0'
info: Microsoft.IdentityModel.LoggingExtensions.IdentityLoggerAdapter[0]
      IDX10245: Creating claims identity from the validated token: '[PII of type 'Microsoft.IdentityModel.JsonWebTokens.JsonWebToken' is hidden. For more details, see https://aka.ms/IdentityModel/PII.]'.
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'HTTP: POST api/messages'
trce: Rido.BFLite.Core.BotApplication[0]
      Reading activity from request body
 {"recipient":{"agenticUserId":"96a64abd-3267-4143-b9a5-194e6f96ef2b","agenticAppId":"5081ddac-3d33-4766-98fe-80c38c5ce554","agenticAppBlueprintId":"9d17cc32-c91b-4368-8494-1b29ccb0dbcf","callbackUri":"https://n3mzm86w-3978.usw2.devtunnels.ms/api/messages","tenantId":"5369a35c-46a5-4677-8ff9-2e65587654e7","id":"8:orgid:96a64abd-3267-4143-b9a5-194e6f96ef2b","name":"RicardoAgent","role":"agenticUser"},"type":"message","id":"1761011073793","timestamp":"2025-10-21T01:44:33.8149889Z","localTimestamp":"2025-10-20T18:44:33.8149889-07:00","localTimezone":"America/Los_Angeles","serviceUrl":"https://cosmic-int.botapi.skype.net/amer/5369a35c-46a5-4677-8ff9-2e65587654e7/","channelId":"msteams","from":{"id":"8:orgid:67c88439-0bdc-4345-b554-f4e5cb74d547","name":"rmpablos","aadObjectId":"67c88439-0bdc-4345-b554-f4e5cb74d547"},"conversation":{"conversationType":"personal","tenantId":"5369a35c-46a5-4677-8ff9-2e65587654e7","id":"19:67c88439-0bdc-4345-b554-f4e5cb74d547_96a64abd-3267-4143-b9a5-194e6f96ef2b@unq.gbl.spaces"},"textFormat":"plain","locale":"en-US","text":"dd","attachments":[{"contentType":"text/html","content":"<p>dd</p>"}],"entities":[{"type":"clientInfo","locale":"en-US","country":"US","platform":"Web","timezone":"America/Los_Angeles"}],"channelData":{"tenant":{"id":"5369a35c-46a5-4677-8ff9-2e65587654e7"}}}

info: Rido.BFLite.Core.BotApplication[0]
      New activity received of type message from 8:orgid:67c88439-0bdc-4345-b554-f4e5cb74d547
trce: Rido.BFLite.Core.ConversationClient[0]
      Sending response to
 POST https://cosmic-int.botapi.skype.net/amer/5369a35c-46a5-4677-8ff9-2e65587654e7/v3/conversations/19:67c88439-0bdc-4345-b554-f4e5cb74d547_96a64abd-3267-4143-b9a5-194e6f96ef2b@unq.gbl.spaces/activities/

 {
        "type": "message",
        "channelId": "msteams",
        "text": "you said dd, with \u2764\uFE0F at 6:44:36 PM",
        "serviceUrl": "https://cosmic-int.botapi.skype.net/amer/5369a35c-46a5-4677-8ff9-2e65587654e7/",
        "replyToId": "1761011073793",
        "from": {
          "id": "8:orgid:96a64abd-3267-4143-b9a5-194e6f96ef2b",
          "name": "RicardoAgent",
          "agenticUserId": "96a64abd-3267-4143-b9a5-194e6f96ef2b",
          "agenticAppId": "5081ddac-3d33-4766-98fe-80c38c5ce554",
          "agenticAppBlueprintId": "9d17cc32-c91b-4368-8494-1b29ccb0dbcf",
          "callbackUri": "https://n3mzm86w-3978.usw2.devtunnels.ms/api/messages",
          "tenantId": "5369a35c-46a5-4677-8ff9-2e65587654e7",
          "role": "agenticUser"
        },
        "recipient": {
          "id": "8:orgid:67c88439-0bdc-4345-b554-f4e5cb74d547",
          "name": "rmpablos",
          "aadObjectId": "67c88439-0bdc-4345-b554-f4e5cb74d547"
        },
        "conversation": {
          "id": "19:67c88439-0bdc-4345-b554-f4e5cb74d547_96a64abd-3267-4143-b9a5-194e6f96ef2b@unq.gbl.spaces",
          "conversationType": "personal",
          "tenantId": "5369a35c-46a5-4677-8ff9-2e65587654e7"
        }
      }


trce: Rido.BFLite.Core.ConversationClient[0]
      Token Claims :
 aud: 0d94caae-b412-4943-8a68-83135ad6d35f
 iss: https://sts.windows.net/5369a35c-46a5-4677-8ff9-2e65587654e7/
 iat: 1761010777
 nbf: 1761010777
 exp: 1761016048
 acr: 0
 aio: AeQAG/8aAAAA/+i/kKjAyJ1wA7/Wf5klB7G+zNlH4paqEb0aYWynXBcb7T58TJu+87JSQ/adUkoCyC5kmTNqbUCoo4Yhq9zM++tnLq0IvW5p/BBIHjBB+NwYF0i/7/iXhaHn50139N23QHsXeAwiCEv/uErKhk0eBD87JJNyYb2bEw9osCTvvmJpr0dMXUQfqHkfTabCIBo2OjRXbCWHphOpFYA6B5BsyDivwCzhJCZvqLc74KhuZqQKcDR6btyXl8JQnRU00m4wB78pVOOjgB2jBJF80hvUNaAa6eF7oPOegABcuBZgSGQ=
 amr: mfa
 appid: 5081ddac-3d33-4766-98fe-80c38c5ce554
 appidacr: 2
 idtyp: user
 ipaddr: 70.37.26.70
 name: RicardoAgent
 oid: 96a64abd-3267-4143-b9a5-194e6f96ef2b
 puid: 100320053645AAC9
 rh: 1.AVkBXKNpU6VGd0aP-S5lWHZU567KlA0StENJimiDE1rW019ZAcRZAQ.
 scp: Authorization.ReadWrite user_impersonation
 sid: 009bd2b9-2337-4512-bf01-8b463b258907
 sub: _EzqEq_xvyUhEdBJDWRLYCohcPvsimsETAaVDMytvEg
 tid: 5369a35c-46a5-4677-8ff9-2e65587654e7
 unique_name: ricardoagent@testcsaaa.onmicrosoft.com
 upn: ricardoagent@testcsaaa.onmicrosoft.com
 uti: XaLWe5vXT06Ro9NATkyYAA
 ver: 1.0
 xms_act_fct: 9 11 1
 xms_ftd: kJmlCabVkL302wzwNE5BIbGCDZMM8Unyla2J14j3qaUBdXNzb3V0aC1kc21z
 xms_idrel: 1 24
 xms_par_app_azp: 9d17cc32-c91b-4368-8494-1b29ccb0dbcf
 xms_sub_fct: 13 1
info: System.Net.Http.HttpClient.Default.LogicalHandler[100]
      Start processing HTTP request POST https://cosmic-int.botapi.skype.net/amer/5369a35c-46a5-4677-8ff9-2e65587654e7/v3/conversations/19:67c88439-0bdc-4345-b554-f4e5cb74d547_96a64abd-3267-4143-b9a5-194e6f96ef2b@unq.gbl.spaces/activities/
info: System.Net.Http.HttpClient.Default.ClientHandler[100]
      Sending HTTP request POST https://cosmic-int.botapi.skype.net/amer/5369a35c-46a5-4677-8ff9-2e65587654e7/v3/conversations/19:67c88439-0bdc-4345-b554-f4e5cb74d547_96a64abd-3267-4143-b9a5-194e6f96ef2b@unq.gbl.spaces/activities/
info: System.Net.Http.HttpClient.Default.ClientHandler[101]
      Received HTTP response headers after 685.9214ms - 403
info: System.Net.Http.HttpClient.Default.LogicalHandler[101]
      End processing HTTP request after 696.2375ms - 403
trce: Rido.BFLite.Core.ConversationClient[0]
      Response Status Forbidden, content {"error":{"code":"ServiceError","message":"Unknown"}}
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'HTTP: POST api/messages'
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.Exception: Error sending activity: Forbidden - {"error":{"code":"ServiceError","message":"Unknown"}}
         at Rido.BFLite.Core.ConversationClient.SendActivityAsync(Activity activity, CancellationToken cancellationToken) in C:\_code\Rido.BFLite\src\Rido.BFLite.Core\ConversationClient.cs:line 45
         at Rido.BFLite.Core.BotApplication.SendActivityAsync(Activity activity) in C:\_code\Rido.BFLite\src\Rido.BFLite.Core\BotApplication.cs:line 121
         at Program.<>c__DisplayClass0_0.<<<Main>$>b__0>d.MoveNext() in C:\_code\Rido.BFLite\samples\Samples.BotEcho\Program.cs:line 16
      --- End of stack trace from previous location ---
         at Rido.BFLite.Core.BotApplication.ProcessAsync(HttpContext httpContext) in C:\_code\Rido.BFLite\src\Rido.BFLite.Core\BotApplication.cs:line 58
         at Rido.BFLite.Core.Hosting.AppBuilderExtensions.<>c__DisplayClass0_0`1.<<UseBotApplication>b__0>d.MoveNext() in C:\_code\Rido.BFLite\src\Rido.BFLite.Core\Hosting\AppBuilderExtensions.cs:line 17
      --- End of stack trace from previous location ---
         at Microsoft.AspNetCore.Http.RequestDelegateFactory.<ExecuteTaskOfString>g__ExecuteAwaited|133_0(Task`1 task, HttpContext httpContext)
         at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://localhost:3978/api/messages - 500 - text/plain;+charset=utf-8 6554.0110ms
