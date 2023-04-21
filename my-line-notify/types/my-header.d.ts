import { LitElement } from 'lit';
/**
 * An example element.
 *
 * @slot - This element has a slot
 * @csspart button - The button
 */
export declare class MyHeader extends LitElement {
    isLogin: boolean;
    constructor();
    connectedCallback(): Promise<void>;
    render(): import("lit-html").TemplateResult<1>;
    private _LineLogout;
    _goHome(e: Event): void;
    _goNotify(e: Event): void;
    _goLogin(e: Event): void;
    static styles: import("lit").CSSResult;
}
declare global {
    interface HTMLElementTagNameMap {
        'my-header': MyHeader;
    }
}
