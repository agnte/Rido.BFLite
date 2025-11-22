# Agentic Setup

## Create the BluePrint

- Install A365 CLI with

```bash
dotnet tool install --global Microsoft.Agents.A365.DevTools.Cli --version 1.0.20-preview
```
 

- Create the A365 Blueprint using the following command:

```bash
a365 config init
a365 setup --bluprint
a365 create-instance identity
a365 create-instance licenses
```

These commands will set up the necessary configurations and create instances for identity and licenses.


You should have the ClientIDs for AB/AI/AU


## Add OAuth Consent

resource-id can be found by querying the service principal for the AppID of the resource you want to grant consent to.


`https://botapi.skype.com` resource-id: `86d5c138-b33a-43f7-805f-59f75e4c699`
`https://api.botframework.com` resource-id: `732b04c4-6d44-4e52-b5c0-d3f38464854a`


```rest
POST https://graph.microsoft.com/beta/oauth2PermissionGrants
Content-Type: application/json

{
    "clientId": "<AI-ClientID>",
    "consentType": "AllPrincipals",
    "resourceId": "<resource-id>",
    "scope": "user_impersonation Authorization.ReadWrite",
}
```




### Teams Backend Resource (SMBA/APX)

```rest
GET https://graph.microsoft.com/v1.0/servicePrincipals?$filter=appId+eq+'5a807f24-c9de-44ee-a3a7-329e88a00ffc'
```

returns: `86d5c138-b33a-43f7-805f-59f75e4c699`
associated to: `https://botapi.skype.com`



### User Token Service (ABS)

```rest
https://graph.microsoft.com/v1.0/servicePrincipals?$filter=appId+eq+'8d2d3342-cf29-4959-9577-0e0eafbd16bc'
```

returns: `732b04c4-6d44-4e52-b5c0-d3f38464854a`
associated to: `https://api.botframework.com`