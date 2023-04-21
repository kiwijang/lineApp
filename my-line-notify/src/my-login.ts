import { LitElement, css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import litLogo from './assets/lit.svg';
import viteLogo from './assets/vite.svg';

/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
@customElement('my-login')
export class MyLogin extends LitElement {
  // <button id="logout-btn" @click=${this._LineLogout} part="button">
  //   登出
  // </button>
  render() {
    return html`
      <div class="login-wrap">
        <h1>登入</h1>

        <h3>免註冊，用 Line 登入</h3>

        <div class="card">
          <button id="login-btn" @click=${this._LineLogin} part="button">
            前往 Line Login
          </button>
        </div>

        <h4 class="gray-text">
          使用 Line Notify 發送通知，<br />
          並記錄你發送過的訊息。
        </h4>

        <div class="bottom">
          <p class="read-the-docs">使用 Vite 和 Lit 製作前端</p>

          <a href="https://vitejs.dev" target="_blank">
            <img src=${viteLogo} class="logo" alt="Vite logo" />
          </a>
          <a href="https://lit.dev" target="_blank">
            <img src=${litLogo} class="logo lit" alt="Lit logo" />
          </a>
        </div>
      </div>
    `;
  }

  private async _LineLogin() {
    // https://developers.line.biz/en/docs/line-login/integrate-line-login/#making-an-authorization-request
    let url = new URL('https://access.line.me/oauth2/v2.1/authorize');

    url.searchParams.append('response_type', 'code');
    url.searchParams.append('response_mode', 'form_post');
    url.searchParams.append('client_id', '1660895465');
    url.searchParams.append('state', 'peko123123');
    url.searchParams.append('nonce', 'abcd5678peko');
    url.searchParams.append(
      'redirect_uri',
      'http://localhost:5000/api/Users/GetToken'
    );
    url.searchParams.append('scope', 'profile openid');

    window.open(url.href, '_self');
  }

  // message API 產生 key
  // genKey() {
  //   (async () => {
  //     const pair = await crypto.subtle.generateKey(
  //       {
  //         name: 'RSASSA-PKCS1-v1_5',
  //         modulusLength: 2048,
  //         publicExponent: new Uint8Array([1, 0, 1]),
  //         hash: 'SHA-256',
  //       },
  //       true,
  //       ['sign', 'verify']
  //     );

  //     console.log('=== private key ===');
  //     console.log(
  //       JSON.stringify(
  //         await crypto.subtle.exportKey('jwk', pair.privateKey),
  //         null,
  //         '  '
  //       )
  //     );

  //     console.log('=== public key ===');
  //     console.log(
  //       JSON.stringify(
  //         await crypto.subtle.exportKey('jwk', pair.publicKey),
  //         null,
  //         '  '
  //       )
  //     );
  //   })();
  // }

  static styles = css`
    :host {
      max-width: 1280px;
      margin: 0 auto;
      padding: 2rem;
      text-align: center;
    }

    h3 {
      letter-spacing: 1px;
    }

    .gray-text {
      color: #aaa;
      font-weight: 400;
      letter-spacing: 1px;
    }

    #login-btn {
      background-color: #646cffaa;
      margin-bottom: 16px;
    }

    .login-wrap {
      margin: auto;
      max-width: 300px;
      padding: 20px;
    }

    .bottom {
      margin-top: 150px;
    }

    .logo {
      height: 24px;
      padding: 10px;
      will-change: filter;
      transition: filter 300ms;
    }
    .logo:hover {
      filter: drop-shadow(0 0 5px #646cffaa);
    }
    .logo.lit:hover {
      filter: drop-shadow(0 0 5px #325cffaa);
    }

    .card {
      display: flex;
      flex-direction: column;
      padding: 2em;
    }

    .read-the-docs {
      color: #888;
    }

    h1 {
      font-size: 3.2em;
      line-height: 1.1;
    }

    a {
      font-weight: 500;
      color: #646cff;
      text-decoration: inherit;
    }
    a:hover {
      color: #535bf2;
    }

    button {
      border-radius: 8px;
      border: 1px solid transparent;
      padding: 0.6em 1.2em;
      font-size: 1em;
      font-weight: 500;
      font-family: inherit;
      background-color: #1a1a1a;
      cursor: pointer;
      transition: border-color 0.25s;
    }
    button:hover {
      border-color: #646cff;
    }
    button:focus,
    button:focus-visible {
      outline: 4px auto -webkit-focus-ring-color;
    }

    #subscribe {
      background-color: #646cffaa;
      margin-bottom: 16px;
    }

    @media (prefers-color-scheme: light) {
      a:hover {
        color: #747bff;
      }
      button {
        background-color: #f9f9f9;
      }
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    'my-login': MyLogin;
  }
}
