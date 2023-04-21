import { Router } from '@vaadin/router';
import { LitElement, css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
@customElement('my-header')
export class MyHeader extends LitElement {
  @property() isLogin = false;

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
        this.isLogin = true;
      } else {
        this.isLogin = false;
      }
      const options = {
        detail: { isLogin: this.isLogin },
        bubbles: true,
        composed: true,
      };
      this.dispatchEvent(new CustomEvent('isLogin', options));
    });
  }
  async connectedCallback() {
    super.connectedCallback();
    document.addEventListener('isLogin', (e: any) => {
      this.isLogin = e.detail.isLogin;
    });
  }

  render() {
    return html`
      <div class="header">
        <a @click="${this._goHome}">home</a>
        <a @click="${this._goNotify}">notify</a>
        <span ?hidden="${this.isLogin}">
          <a id="login" @click="${this._goLogin}">登入</a>
        </span>
        <span ?hidden="${!this.isLogin}">
          <a id="logout" @click="${this._LineLogout}">登出</a>
        </span>
      </div>
      <slot></slot>
    `;
  }

  private async _LineLogout() {
    // https://developers.line.biz/en/docs/line-login/integrate-line-login/#making-an-authorization-request
    // post https://notify-api.line.me/api/revoke
    fetch('http://localhost:5000/api/Users/RevokeLogin', {
      method: 'GET',
      mode: 'cors',
      cache: 'no-cache',
      credentials: 'include',
    });
    this.isLogin = false;
    window.alert('登出成功!');
    Router.go('/login');
  }

  _goHome(e: Event) {
    e.preventDefault();
    Router.go('/');
  }

  _goNotify(e: Event) {
    e.preventDefault();
    Router.go('/notify');
  }
  _goLogin(e: Event) {
    e.preventDefault();
    Router.go('/login');
  }

  static styles = css`
    :host {
      display: flex;
      justify-content: center;
    }

    .header {
      padding: 20px;
    }

    #logout {
      color: chocolate;
      text-decoration: underline;
      text-underline-offset: 5px;
    }

    #login {
      text-decoration: underline;
      text-underline-offset: 5px;
    }

    a {
      display: inline-block;
      padding: 10px;
      letter-spacing: 1px;
      text-decoration: none;
      transition: 500ms;
      cursor: pointer;
    }
    a:hover {
      transition: 1s;
      opacity: 0.5;
    }
  `;
}

declare global {
  interface HTMLElementTagNameMap {
    'my-header': MyHeader;
  }
}
