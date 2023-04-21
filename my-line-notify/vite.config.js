import { defineConfig } from 'vite';
import { babel } from '@rollup/plugin-babel';

// https://vitejs.dev/config/
export default defineConfig({
  build: {
    lib: {
      entry: 'dist/index.js',
      formats: ['es'],
    },
    minify: 'esbuild',
    rollupOptions: {
      // external: /^lit/,
      input: ['index.html'],
      output: {
        entryFileNames: 'entry-[name].js',
        format: 'iife',
        // sourcemap: true,
      },
      plugins: [
        babel({
          exclude: 'node_modules/**',
          configFile: './babel.config.json',
          babelHelpers: 'bundled',
        }),
      ],
    },
  },
  server: {
    port: 3030,
    host: 'localhost',
  },
  preview: {
    port: 3030,
    host: 'localhost',
  },
});
