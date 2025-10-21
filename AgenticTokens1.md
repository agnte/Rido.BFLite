# AgenticTokens from Console

Given these values

```
string tenantId = "5369a35c-46a5-4677-8ff9-2e65587654e7";
string clientId = "9d17cc32-c91b-4368-8494-1b29ccb0dbcf";
string secret = "BfX8Q***********************";
string fmiPath = "5081ddac-3d33-4766-98fe-80c38c5ce554";
string userId = "96a64abd-3267-4143-b9a5-194e6f96ef2b";
```

Token1

```json
{
  "aud": "fb60f99c-7a34-4190-8149-302f77469936",
  "iss": "https://login.microsoftonline.com/5369a35c-46a5-4677-8ff9-2e65587654e7/v2.0",
  "iat": 1760988998,
  "nbf": 1760988998,
  "exp": 1760992898,
  "aio": "k2JgYAjuXPb29rw5QY0JoaHm6QKBYfecPe9IOJWLNp/W0mEOPwEA",
  "azp": "9d17cc32-c91b-4368-8494-1b29ccb0dbcf",
  "azpacr": "1",
  "idtyp": "app",
  "oid": "8b9ea2e4-534a-46bb-b3c7-4027969d271c",
  "rh": "1.AVkBXKNpU6VGd0aP-S5lWHZU55z5YPs0epBBgUkwL3dGmTZZAQBZAQ.",
  "sub": "/eid1/c/pub/t/XKNpU6VGd0aP-S5lWHZU5w/a/MswXnRvJaEOElBspzLDbzw/5081ddac-3d33-4766-98fe-80c38c5ce554",
  "tid": "5369a35c-46a5-4677-8ff9-2e65587654e7",
  "uti": "OQZCL2CY80uXMgjZyL4OAA",
  "ver": "2.0",
  "xms_act_fct": "9 1",
  "xms_ficinfo": "CAAQABgAIAAoAjAA",
  "xms_ftd": "GhaRO28H57EdxZG63sWY6yJnlMefV-2oUGtsSqR0mVwBdXNzb3V0aC1kc21z",
  "xms_idrel": "14 7",
  "xms_sub_fct": "1 9"
}
```

Token 2

```json
{
  "aud": "fb60f99c-7a34-4190-8149-302f77469936",
  "iss": "https://login.microsoftonline.com/5369a35c-46a5-4677-8ff9-2e65587654e7/v2.0",
  "iat": 1760989039,
  "nbf": 1760989039,
  "exp": 1760992939,
  "aio": "ASQA2/8aAAAAwsuG+2Djr9fdNb1eVcrI4CYsMwBzV86QUSkRkMdZKEI=",
  "azp": "5081ddac-3d33-4766-98fe-80c38c5ce554",
  "azpacr": "2",
  "idtyp": "app",
  "oid": "5081ddac-3d33-4766-98fe-80c38c5ce554",
  "rh": "1.AVkBXKNpU6VGd0aP-S5lWHZU55z5YPs0epBBgUkwL3dGmTZZAQBZAQ.",
  "sub": "5081ddac-3d33-4766-98fe-80c38c5ce554",
  "tid": "5369a35c-46a5-4677-8ff9-2e65587654e7",
  "uti": "maB3hBIhg0CQBpx3VazvAA",
  "ver": "2.0",
  "xms_act_fct": "1 9 11",
  "xms_ficinfo": "CAAQABgAIAAoAzAAOAE",
  "xms_ftd": "4s-3q88TFTIp7t5D2FBzrZHSo3p5NjIASQP-apRjuDoBdXN3ZXN0My1kc21z",
  "xms_idrel": "7 12",
  "xms_par_app_azp": "9d17cc32-c91b-4368-8494-1b29ccb0dbcf",
  "xms_sub_fct": "11 1 9"
}
```

Final Token

```json
{
  "aud": "https://graph.microsoft.com",
  "iss": "https://sts.windows.net/5369a35c-46a5-4677-8ff9-2e65587654e7/",
  "iat": 1760989116,
  "nbf": 1760989116,
  "exp": 1761018216,
  "acct": 0,
  "acr": "0",
  "acrs": [
    "p1"
  ],
  "aio": "AeQAG/8aAAAA4jgNTLwgx1omzK71rPakvxY3je1omTTROQiKrHulaAq/9VRokkcN057BoZc9e0ZRsf8pnFUn8bpVf3oySrUqJl/P2BYVc0d1bZX6rsjkR6CmaiyfUWINHdZXfBZrAoKLoKObhpQBsgXjHQ3ywWrfxAP3FUhHF35UX5Uau8DtgV/UkDZZALXD3ZFxk7EKC+LxiEcGC4qFMaUkmiOFg6AhmGh99HNLBh6e0ojrtDMz6kcey7L6tbVYi8lrWMnePDl/G7ypnvel2zBFbz1L61E3RX6/tDiVP1kwBxMs5mYG0eU=",
  "amr": [
    "mfa"
  ],
  "app_displayname": "RicardoAgentAppInstance-1",
  "appid": "5081ddac-3d33-4766-98fe-80c38c5ce554",
  "appidacr": "2",
  "idtyp": "user",
  "ipaddr": "70.37.26.70",
  "name": "RicardoAgent",
  "oid": "96a64abd-3267-4143-b9a5-194e6f96ef2b",
  "platf": "3",
  "puid": "100320053645AAC9",
  "rh": "1.AVkBXKNpU6VGd0aP-S5lWHZU5wMAAAAAAAAAwAAAAAAAAABZAcRZAQ.",
  "scp": "Chat.ReadWrite ChatMessage.Send https://graph.microsoft.com/.default User.Read profile openid email",
  "sid": "009baba9-b146-bba0-e940-56587ff70244",
  "sub": "JoJDPSfSPdM0R3ApdiVyqBPXbmweTBoY0tojAuIGUl0",
  "tenant_region_scope": "NA",
  "tid": "5369a35c-46a5-4677-8ff9-2e65587654e7",
  "unique_name": "ricardoagent@testcsaaa.onmicrosoft.com",
  "upn": "ricardoagent@testcsaaa.onmicrosoft.com",
  "uti": "LnIQZKRVVE21wc_TgY8jAQ",
  "ver": "1.0",
  "wids": [
    "b79fbf4d-3ef9-4689-8143-76b194e85509"
  ],
  "xms_act_fct": "1 9 11",
  "xms_ftd": "PpC0b0s2HobdCkKoCtxcUM9cKzsoI1_hoDGIPn0kgREBdXNub3J0aC1kc21z",
  "xms_idrel": "12 1",
  "xms_par_app_azp": "9d17cc32-c91b-4368-8494-1b29ccb0dbcf",
  "xms_st": {
    "sub": "_d7BCn5kn4TnkCcb9ezFxyVBslS1ZyHih6084yALexw"
  },
  "xms_sub_fct": "1 13",
  "xms_tcdt": 1754409920,
  "xms_tnt_fct": "6 1"
}
```

https://login.microsoftonline.com/5369a35c-46a5-4677-8ff9-2e65587654e7/oauth2/v2.0/authorize?client_id=5081ddac-3d33-4766-98fe-80c38c5ce554&scope=https://api.botframework.com/.default&redirect_uri=https://entra.microsoft.com/TokenAuthorize&response_type=none&prompt=consent

