import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    environment: 'jsdom',     // provide document/window
    globals: true,            // vitest globals (describe/it/expect) available
    setupFiles: './src/setupTests.ts',
    include: ['src/**/*.test.{ts,tsx}', 'src/**/*.spec.{ts,tsx}']
  },
});