import { Router } from '@vaadin/router';

function initRouter() {
  const router = new Router(document.querySelector('#app'));

  router.setRoutes([
    {
      path: '/',
      component: 'my-home',
      action: async () => await import('./my-home'),
    },
    {
      path: '/login',
      component: 'my-login',
      action: async () => await import('./my-login'),
    },
    {
      path: '/notify',
      component: 'my-notify',
      action: async () => await import('./my-notify'),
    },
    { path: '(.*)', redirect: '/' }, // 萬用路由
  ]);
}

window.addEventListener('DOMContentLoaded', () => {
  initRouter();
});
