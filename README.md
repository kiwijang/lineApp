# lineApp

```
cd my-line-notify
npm i
npm run dev
```

```
cd API
dotnet run
```


# 關於此通知系統

## 概述

此通知系統必須登入 Line Login 才能進行訂閱與發通知，
透過串接 Line Notify 服務，讓使用者可以訂閱特定頻道的放送內容。
為了儲存使用者的訂閱資訊，系統使用 sqlite 作為資料庫。

## 功能與特色

使用者必須登入 Line Login 才能訂閱 (Line Login 才有 Id Token 且 Access Token 預設 30 天會過期)，
系統會記錄使用者的 Notify 訊息內容與時間。
為了保護使用者的個資，系統不會儲存使用者的個人資訊，
只會存取 sub 和 line 顯示名稱。

通知系統具有以下特色：

- 使用 AES 加密過的 Line Notify access_token 儲存 Notify 訂閱資訊，以避免重複訂閱。
- 使用 Cookie 紀錄 Notify access_token 以及使用者登入狀態，保障使用者的資訊安全。
- 使用 EF Core Code First 設計 DB schema，建立關聯式資料庫。
- 透過 OAuth 實現簡單的身份驗證。

## 技術細節

### 資料庫設計

![image](https://user-images.githubusercontent.com/21300139/233802141-34a01111-3629-417e-a90c-6e04af4d8d62.png)
> DB schema

系統使用 sqlite 作為資料庫，設計了三個資料表：

- Users：使用者資料，使用 jwt 的 sub 為 PK。
- NotifyData：儲存 AES 加密過後的 Line Notify access_token，使用者登入後即建立資料。
- NotifyHist：儲存發文資料與時間點，用於系統追蹤和紀錄。

啟動應用時就 migrate sqlite DB 一次。
最酷的是關聯設計，可參考以下兩篇文章。
- [EF Core 筆記 3 - Model 關聯設計 by 暗黑執行續](https://blog.darkthread.net/blog/ef-core-notes-3/)
- [CodeFirst與資料庫版控 by ATai](https://blog.darkthread.net/blog/ef-core-notes-3/)

### 前端技術

前端採用 Web Component 技術，父子元件之間使用事件傳遞機制進行參數傳遞。
系統的前端也使用 Vite Library Mode 作為打包工具。

### 後端技術

系統的後端是以 ASP.NET Core 為基礎所建構，使用 EF Core Code First 進行 DB schema 的建立。
為了確保使用者資料的安全性，系統使用 JWT 和 RSA 進行身份驗證和加密，同時使用 Cookie 儲存 Notify access_token。

### Cookies

Cookie 在前後端都有設定:

- 前端
``` javascript
 fetch('https://notify-api-prac.azurewebsites.net/api/Users/VerifyLogin', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    });
```

- 後端
``` javascript
// 將 notify access_token 存到 Cookie
var cookieOptions = new CookieOptions
{
  HttpOnly = true, // 僅可通過 HTTP 訪問
  SameSite = SameSiteMode.None,
  Secure = true,
  Expires = DateTimeOffset.Now.AddDays(30), // Line  notify 預設值不會過期，同 line login 30 天
};
// 寫入 Cookie
Response.Cookies.Append("notify", result.access_token, cookieOptions);
```

限制網域可以這樣寫:
```
SameSite = SameSiteMode.Lax,
Domain = "xxx.com"
```

### Web Component 參數傳遞

- 父層
```
document.addEventListener('isLogin', (e: any) => {
  this.isLogin = e.detail.isLogin;
});
```

- 子層
```
 const options = {
   detail: { isLogin: this.isLoginAccessTokenValid },
   bubbles: true,
   composed: true,
 };
 this.dispatchEvent(new CustomEvent('isLogin', options));
```

### Vite Library Mode

Vite 是一款快速的前端開發工具，並且支援 Library Mode。Vite Library Mode 是指在這個工具中，
開發者可以快速地創建可用於其他專案的 JavaScript 函式庫。
透過這個模式，開發者可以使用 Vite 的優勢來開發高效的 JavaScript 函式庫，
並在需要使用它們的專案中輕鬆引入。
舉例來說，假設你想要創建一個名為 my-library 的 JavaScript 函式庫，
該函式庫提供了一些常用的工具函式。使用 Vite Library Mode，
你可以快速地為這個函式庫創建一個開發環境，並使用 Vite 的優勢進行開發和調試。
完成後，你可以將該函式庫發佈到 NPM 上，供其他專案使用。

https://vitejs.dev/guide/build.html

### .babelrc 和 babel.config.json

在 Babel 7.x 的新版本中，Babel 引入了 "root" 目錄的概念，預設為當前的工作目錄。對於整個項目的配置，Babel 將自動在根目錄中搜索 "babel.config.json" 文件，或者是具有支援的擴展名的等效文件。此外，用戶還可以使用顯式的 "configFile" 值覆蓋預設的配置文件搜索行為。

https://babel.docschina.org/docs/en/config-files/
> 假設您已經按照上面討論的方式正確載入了 babel.config.json 檔案，Babel 只會處理根套件內的 .babelrc.json 檔案（而不是子套件）。
> 要啟用該 .babelrc.json 的處理，您需要從 babel.config.json 檔案內使用 "babelrcRoots" 選項。

## 對稱加密和非對稱加密

### JWT、RSA

- JWT
JWT (JSON Web Token) 是一種用於身份驗證和授權的開放標準 (RFC 7519)，它使用了對稱加密的方式。JWT 包含三個部分：header、payload 和 signature。其中，header 和 payload 都是 base64 編碼的字符串，可以被任何人解碼。而 signature 則是使用私鑰對 header 和 payload 進行簽名，以保證它們不被篡改。在 JWT 被傳輸到服務器端進行驗證時，服務器會使用公鑰對 signature 進行驗證，以確定 JWT 是否經過正確的簽名，從而驗證用戶身份。
總結起來，JWT 的公私鑰的作用如下：

公鑰：用於驗證 JWT 的 signature 是否有效。
私鑰：用於對 JWT 的 header 和 payload 進行簽名。

- RSA
RSA 是一種非對稱加密算法，它使用了一對公私鑰來進行加密和解密。RSA 的公鑰可以被任何人使用，用於加密數據；而私鑰則只有擁有者可以使用，用於解密數據。通常在使用 RSA 進行加密時，會將要加密的數據和公鑰一起傳遞給接收者，接收者再使用私鑰對數據進行解密。在這個過程中，公鑰的作用是保證只有接收者才能解密數據，而私鑰的作用則是確保數據在傳輸過程中不被非法獲取。
總結起來，RSA 的公私鑰的作用如下：

公鑰：用於加密數據。
私鑰：用於解密數據。

### AES、HS256

對稱加密算法是指加密和解密使用相同密鑰的加密算法。這意味著在使用對稱加密算法加密數據時，需要將密鑰傳遞給數據的接收者，以便對數據進行解密。

AES（Advanced Encryption Standard）是一種流行的對稱加密算法。它支持不同密鑰長度，其中最常見的是128位、192位和256位。AES 128位是最常見的密鑰長度，也是最常用的加密級別。

HS256是JWT（JSON Web Token）中使用的一種哈希算法，它是基於SHA-256哈希函數的對稱加密算法。在JWT中，使用HS256算法將信息與密鑰一起進行簽名，接收方可以使用相同的密鑰驗證簽名的有效性。由於使用相同密鑰對數據進行加密和解密，因此HS256算法是對稱加密算法。

## OAuth 簡介

OAuth是一種用於授權的協議，用於在不直接提供用戶名和密碼的情況下，允許一個應用程序或服務訪問另一個應用程序或服務的用戶資源。

OAuth協議的基本思想是，當用戶想要授權一個應用程序訪問其資源時，
用戶被重定向到授權服務器，用戶在授權服務器上輸入自己的賬戶信息，
該服務器會請求用戶授權應用程序訪問特定資源，並根據用戶的授權將訪問令牌返回給應用程序。
應用程序隨後使用此令牌訪問用戶的資源，直到令牌過期或被撤銷。

OAuth通常被用於實現第三方登錄，以及在多個應用程序之間共享用戶資源。它是一種安全的方式，可以為用戶提供更好的控制權，同時避免了將密碼直接提供給第三方應用程序的風險。

OAuth 的 RFC 規格書為 [RFC6749](https://www.rfc-editor.org/rfc/rfc6749#appendix-A.10)。
OAuth2.0 定義了五種不同的授權流程，以適應不同的應用場景和用戶需求。這些授權流程包括：

1. 授權碼流程（Authorization Code Grant）：最廣泛使用的授權流程，適用於第三方應用程序需要訪問用戶數據的情況。該流程分為獲取授權碼和使用授權碼獲取訪問令牌兩個步驟。

1. 隱式授權流程（Implicit Grant）：適用於簡單的用戶場景，例如前端 JavaScript 應用程序需要訪問某些用戶數據的情況。該流程僅通過重定向 URI 返回訪問令牌，不經過授權碼這個中介步驟。

1. 密碼授權流程（Resource Owner Password Credentials Grant）：適用於應用程序只信任用戶，不信任第三方的情況。用戶需要直接將自己的資格證明發送到應用程序中，該流程的安全性較低，並不推薦在生產環境中使用。

1. 客戶端憑證授權流程（Client Credentials Grant）：適用於第三方應用程序需要訪問受保護的 API 資源，但不需要訪問特定用戶數據的情況。該流程只需要使用應用程序的客戶端 ID 和密鑰就能獲取訪問令牌，安全性較高。

1. 裝置授權授權碼(Device Authorization Grant) 也稱為 Device Flow：用於無法直接輸入密碼的裝置，例如智能電視或游戲機等。在此授權流程中，用戶首先在裝置上輸入驗證 URL，然後使用此 URL 登錄其帳戶並授予裝置權限。此後，裝置可以使用 Access Token 和 Refresh Token 訪問 API 來代表用戶執行操作。
OAuth 2.0 Device Authorization Grant 的規格在 [RFC8628](https://www.rfc-editor.org/rfc/rfc8628)。

## OpenID Connect 簡介

- OpenID Connect (OIDC) 是建立在 OAuth 2.0 基礎上的身份認證協議，使用 JWT 來傳遞身份信息，
其中 ID Token 是包含身份信息的 JWT，
其中定義了多種標準的 Claim，例如 sub (Subject) 代表唯一的用戶識別符，iss (Issuer) 代表發行者，aud (Audience) 代表接收方等等。
例如，當使用 Google 登錄應用時，ID Token 可以包含用戶的姓名、電子郵件地址、唯一的用戶識別符等身份信息。

- JWT (JSON Web Token) 是一種開放標準 (RFC 7519)，用於安全地傳輸資訊。它是由三個部分所組成的字串，分別為 Header、Payload (也稱為 Claim)、Signature。

- JWS (JSON Web Signature) 是 JWT 的一個標準，它可以對 JWT 進行簽名，以確保 JWT 在傳輸過程中不被竄改。JWS 的簽名包含一個 Header 和一個 Payload，通過一個密鑰對這兩部分進行 HMAC-SHA256 或 RSA-SHA256 的簽名處理，產生簽名字串。

- JWE (JSON Web Encryption) 是 JWT 的另一個標準，它可以對 JWT 進行加密，以保護 JWT 中的敏感資訊不被獲取。JWE 的加密可以使用對稱或非對稱加密算法，並且可以使用 JWK (JSON Web Key) 來進行加密金鑰的交換。

- JWKS (JSON Web Key Set) 是一個包含公開金鑰 (Public Key) 的 JSON 物件集合，用於在 JWT 的發行方和驗證方之間進行公開金鑰的交換。

- JWK 代表 JSON Web Key，是一種用於表示對稱和非對稱密鑰的 JSON 格式。JWK 可以包含公鑰、私鑰和對稱密鑰，以及用於識別特定鍵的元數據，如密鑰 ID 和密鑰用途。JWK 能夠與其他 JSON Web 技術（如 JWT、JWS、JWE）一起使用，提供安全的身份驗證和數據保護機制。

## 驗證 jwt 簽章是否正確

> 在伺服器端，直接透過下載過的 JWKs 金鑰驗證使用者發來的 ID Token，確保驗證 ID Token 的效能與可靠性。

- [[OpenID] 使用 RS256 與 JWKS 驗證 JWT token 有效性](https://fullstackladder.dev/blog/2023/01/28/openid-validate-token-with-rs256-and-jwks/)
- [LINE Login 簽發的 ID Token 如何用 ES256 非對稱加密演算法的公開金鑰驗證](https://blog.miniasp.com/post/2023/04/09/How-to-validate-LINE-Login-issued-ES256-ID-Token)

### Access Token 與 Id Token

Access Token 和 Id Token 都是 OpenID Connect(OIDC) 協議中的概念，通常用於驗證用戶身份和授權訪問。

Access Token 是 OIDC 用於授權訪問 API 的憑證。當用戶通過 OAuth 2.0 驗證授權後，經授權的應用程序將收到一個 Access Token，該 Token 包含了訪問資源的權限和期限。當應用程序訪問受保護的 API 時，必須將 Access Token 與每個 API 請求一起發送，以驗證應用程序對該資源的授權。

Id Token 則是 OIDC 用於身份驗證的憑證。它是一個 JSON Web Token（JWT），包含有用戶身份信息和其他附加信息。當用戶通過 OAuth 2.0 授權驗證後，他們的瀏覽器將被重定向回 OIDC 服務器以獲取一個 Id Token。這個 Token 包含了用戶的身份信息，例如用戶的 ID、名字、電子郵件地址等等。應用程序可以使用 Id Token 驗證用戶身份並執行必要的操作。


## 待補

### 檢查 cookie 是否存在應該要抽出去寫 attribute 會比較乾淨。

用 ASP.NET Core 的 Web API 中的自訂授權屬性，以驗證 Cookie 是否存在：
``` csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CookieAuthAttribute : AuthorizeAttribute
{
    public override void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey("myCookieName"))
        {
            context.Result = new UnauthorizedResult();
        }
        else
        {
            base.OnAuthorization(context);
        }
    }
}

```

``` csharp
[ApiController]
[Route("[controller]")]
[CookieAuth]
public class MyController : ControllerBase
{
    // ...
}

```

### state 要防 CSRF(XSRF)，實作 session

CSRF (Cross-Site Request Forgery)，又稱為 XSRF，是一種網站應用程式漏洞攻擊。
攻擊者通過欺騙使用者在瀏覽器中執行非預期的操作，從而將惡意請求發送到被攻擊者已經登錄的網站。
攻擊者可以使用 CSRF 攻擊來執行各種攻擊，如盜取用戶資料、更改用戶資料、傳播惡意軟體等。
常見的防範 CSRF 攻擊的方法包括使用 CSRF token、檢查 Referer header、限制 HTTP 方法等。

在 ASP.NET Core 中，可以使用 session 來實現檢查 state 是否一致。
具體來說，可以在生成授權請求時，在 session 中保存一個隨機的 state 字符串。
當用戶返回後，再從 session 中獲取之前保存的 state 字符串，
與 OAuth 服務器返回的 state 參數進行比對，以確保兩者一致。
如果不一致，則可以返回一個錯誤頁面或者直接拒絕該請求。以下是一個使用 session 驗證 state 的示例：

``` csharp
[HttpGet]
public IActionResult Authorize()
{
    string state = Guid.NewGuid().ToString("N");
    HttpContext.Session.SetString("OAuthState", state);

    var redirectUri = $"https://oauth-server.com/authorize?response_type=code&client_id=1234&redirect_uri=https://client-app.com/callback&state={state}";

    return Redirect(redirectUri);
}

[HttpGet]
public async Task<IActionResult> Callback(string code, string state)
{
    string savedState = HttpContext.Session.GetString("OAuthState");
    if (savedState != state)
    {
        return BadRequest("Invalid state parameter");
    }

    // 使用 code 獲取 access token
    // ...

    return Ok();
}
```

在上面的示例中，Authorize 方法生成授權請求時，生成一個隨機的 state 字符串並保存到 session 中。
當用戶返回後，Callback 方法從 querystring 中獲取 state 參數，並從 session 中獲取之前保存的 state 字符串進行比對。
如果兩者不一致，則返回 BadRequest 結果；否則進行後續處理。

### static web 的路由問題(會把網址當成 http get)

除了可用 HashLocationStrategy
也可這樣轉成 query string 和 hash fragment:
[Routing on github pages doesn't work. How do I fix it?](https://stackoverflow.com/questions/62483227/routing-on-github-pages-doesnt-work-how-do-i-fix-it?fbclid=IwAR1HiW9P8divG0ke6eQ3Mg-aVOcJW1ZOK3A5oMAHoUKIzZbZOzkf1MCsbyQ)
