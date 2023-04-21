import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import litLogo from './assets/lit.svg';
import viteLogo from './assets/vite.svg';
import { Router } from '@vaadin/router';

/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
@customElement('my-notify')
export class MyNotify extends LitElement {
  /**
   * Copy for the read the docs hint.
   */
  @property()
  docsHint = '使用 Vite 和 Lit 製作前端';

  @property() isLoginAccessTokenValid = false;

  constructor() {
    super();

    // 檢查是否有 login
    fetch('http://localhost:5000/api/Users/VerifyLogin', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    }).then((res) => {
      if (res.status == 200 || res.status == 204) {
        this.isLoginAccessTokenValid = true;
        return;
      }
      this.isLoginAccessTokenValid = false;

      const options = {
        detail: { isLogin: this.isLoginAccessTokenValid },
        bubbles: true,
        composed: true,
      };
      this.dispatchEvent(new CustomEvent('isLogin', options));
      Router.go('/login');
    });
  }

  connectedCallback() {
    super.connectedCallback();

    const queryParams = new URLSearchParams(window.location.search);
    const code = queryParams.get('code');
    const state = queryParams.get('state');
    if (code && state) {
      fetch('http://localhost:5000/api/Users/GetNotifyToken', {
        method: 'POST',
        mode: 'cors',
        cache: 'no-cache',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
        },
        body: `code=${code}&state=${state}`,
      }).then((res) => {
        if (res.status === 200) {
          Router.go('/');
        }
      });
    }
  }

  render() {
    if (!this.isLoginAccessTokenValid) return;

    return html`
      <div class="notify-wrap">
        <h1>Line Notify</h1>

        <div class="card">
          <button id="subscribe" @click=${this._GetNotifyCode} part="button">
            訂閱 LINE Notify 通知
          </button>
          <button @click=${this._RevokeNotify} part="button">
            取消 LINE Notify 通知
          </button>
        </div>

        <div class="bottom">
          <p class="read-the-docs">${this.docsHint}</p>

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

  private async _GetNotifyCode() {
    // 檢查是否有 notify
    fetch('http://localhost:5000/api/Users/GetNotifyStatus', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    }).then(async (res) => {
      if (res.status == 200 || res.status == 204) {
        // 已有合法 notify 就不重新訂閱
        window.alert('已訂閱成功!');
        return;
      } else if ((await res.text()) === 'notLogin') {
        this.isLoginAccessTokenValid = false;
        const options = {
          detail: { isLogin: this.isLoginAccessTokenValid },
          bubbles: true,
          composed: true,
        };
        this.dispatchEvent(new CustomEvent('isLogin', options));

        Router.go('/login');
        return;
      }
      this._getCode();
    });
  }

  private _getCode() {
    let url = new URL('https://notify-bot.line.me/oauth/authorize');

    // 欄位說明 https://notify-bot.line.me/doc/en/
    url.searchParams.append('response_type', 'code');
    url.searchParams.append('client_id', 'DfXzLnaKcOdrCOWpa8FLbU');
    url.searchParams.append('state', '123123');
    url.searchParams.append('scope', 'notify');
    url.searchParams.append('redirect_uri', 'http://localhost:3030/notify');

    window.open(url, '_self');
  }

  private _RevokeNotify() {
    // https://developers.line.biz/en/docs/line-login/integrate-line-login/#making-an-authorization-request
    // post https://notify-api.line.me/api/revoke
    fetch('http://localhost:5000/api/Users/RevokeNotify', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    }).then((res) => {
      if (res.status === 200) {
        window.alert('取消訂閱成功!');
      } else {
        window.alert('取消訂閱成功!');
      }
    });
  }

  static styles = css`
    :host {
      max-width: 1280px;
      margin: 0 auto;
      padding: 2rem;
      text-align: center;
    }

    .notify-wrap {
      margin: auto;
      max-width: 300px;
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
    'my-notify': MyNotify;
  }
}
