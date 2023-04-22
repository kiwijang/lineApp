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


## 關於

#### 一定要登入才可訂閱(因為要記錄你的 Notify 訊息內容和時間)

串接 Line Notify 登入，不必儲存使用者個資(但這系統會存 sub 和 line 顯示名字)。
串接 Line Notify 服務，可接收該頻道的放送內容。


#### 使用 sqlite

![image](https://user-images.githubusercontent.com/21300139/233802141-34a01111-3629-417e-a90c-6e04af4d8d62.png)
> DB schema

- Users

使用者資料，來自 jwt 的 sub 為 PK。

- NotifyData

儲存 AES 加密過後的 Line Notify access_token，
因為 Line Notify access_token 永不過期且可重複訂閱，
只要使用者登入就應該不能重複訂閱，才合理。

- NotifyHist

儲存發文資料與時間點。

## 技術

### EF Core Code First

啟動應用時就 migrate sqlite DB 一次。
最酷的是關聯設計，可參考以下兩篇文章。
- [EF Core 筆記 3 - Model 關聯設計 by 暗黑執行續](https://blog.darkthread.net/blog/ef-core-notes-3/)
- [CodeFirst與資料庫版控 by ATai](https://blog.darkthread.net/blog/ef-core-notes-3/)

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

### .babelrc 和 babel.config.json

## 對稱加密和非對稱加密

### JWT、RSA

### AES、HS256

## OAuth 簡介

### Access Token 與 Id Token

## 待補
[Auth] 檢查 cookie
