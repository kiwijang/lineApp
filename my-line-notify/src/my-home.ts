import { Router } from '@vaadin/router';
import { LitElement, css, html } from 'lit';
import { customElement, property, query } from 'lit/decorators.js';
/**
 * 有 Line Login 和 Line Notify 才可以用這頁的功能
 * 發送 notify 與看到歷史記錄
 */
@customElement('my-home')
export class MyHome extends LitElement {
  @query('#textarea') textarea?: HTMLTextAreaElement;

  @property()
  private _listItems: any[] = [];

  @property() isNotifyAccessTokenValid = false;
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

    // 檢查是否有 notify
    fetch('http://localhost:5000/api/Users/GetNotifyStatus', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    }).then((res) => {
      if (res.status == 200 || res.status == 204) {
        this.isNotifyAccessTokenValid = true;
        return;
      }
      this.isNotifyAccessTokenValid = false;
      Router.go('/notify');
    });
  }
  connectedCallback() {
    super.connectedCallback();

    this._updHist();
  }

  async _updHist() {
    const res = await fetch('http://localhost:5000/api/Users/GetNotifyHist', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json; charset=UTF-8',
      },
    });
    this._listItems = await res.json();
  }

  render() {
    if (!this.isLoginAccessTokenValid || !this.isNotifyAccessTokenValid) return;

    return html`
      <div class="home-wrap">
        <h1>發送推播訊息</h1>
        <div class="card">
          可發送訊息給所有訂閱的使用者，顯示每次推播訊息的發送記錄。
        </div>
        <div class="box">
          <textarea id="textarea"></textarea>
          <button id="subscribe" @click=${this._onClick} part="button">
            送出 Notify
          </button>
        </div>
        <div class="box box2">
          <h2>你的推播紀錄👻</h2>
          <ul>
            ${this._listItems?.length > 0
              ? this._listItems?.map(
                  (item) => html` <li>
                    <div class="time">${item.createTime}</div>
                    <div class="content">${item.content}</div>
                  </li>`
                )
              : '目前沒有任何 notify \\(U_U)>'}
          </ul>
        </div>
      </div>
    `;
  }

  private async _onClick() {
    if (!this.textarea?.value.trim()) return;
    // 檢查是否有 notify
    fetch('http://localhost:5000/api/Users/GetNotifyStatus', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    }).then((res) => {
      if (res.status == 200 || res.status == 204) {
        this.isNotifyAccessTokenValid = true;
        // 驗證成功才可送出 notify
        this._notify();
      } else {
        this.isNotifyAccessTokenValid = false;
        return;
      }
    });
  }

  private async _notify() {
    // https://notify-api.line.me/api/notify
    const res = await fetch('http://localhost:5000/api/Users/NotifyMsg', {
      method: 'POST',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
      },
      body: `message=${this.textarea!.value}`,
    });

    if (res) {
      this.textarea!.value = '';
      await this._updHist();

      window.alert('發送成功!');
    }
  }

  static styles = css`
    :host {
      max-width: 1280px;
      margin: 0 auto;
      padding: 2rem;
      text-align: center;
    }

    .home-wrap {
      padding: 20px;
    }

    .box {
      margin: auto;
      max-width: 600px;
      display: flex;
      flex-direction: column;
    }

    .box2 {
      margin-top: 40px;
    }

    ul {
      margin-top: 0;
      padding: 0;
    }

    h2 {
      text-decoration: underline;
      text-underline-offset: 5px;
    }

    li {
      list-style-type: none;
      margin-top: 20px;
      border-bottom: 1px solid gray;
      padding-bottom: 20px;
    }

    .time {
      display: inline-block;
      background-color: black;
      padding: 4px 12px;
      border-radius: 8px;
      margin-bottom: 12px;
    }

    .content {
      letter-spacing: 1px;
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

    textarea {
      min-height: 200px;
      padding: 20px;
      font-size: 16px;
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
      margin-top: 24px;
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
    'my-home': MyHome;
  }
}
