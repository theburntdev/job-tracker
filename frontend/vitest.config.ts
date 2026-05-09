import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test-setup.ts'],
      env: {
        VITE_API_URL: 'http://localhost:5000',
      },
      coverage: {
        provider: 'v8',
        include: ['src/**/*.{ts,tsx}'],
        exclude: [
          'src/lib/api.types.gen.ts',
          'src/main.tsx',
          'src/App.tsx',
          'src/test-utils/**',
          '**/*.d.ts',
          '**/*.test.*',
          'src/styles/**',
        ],
        thresholds: {
          lines: 70,
          functions: 70,
          branches: 60,
          statements: 70,
        },
        reporter: ['text', 'lcov', 'html'],
      },
    },
  }),
)
