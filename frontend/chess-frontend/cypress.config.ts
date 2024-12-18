import { defineConfig } from "cypress";

export default defineConfig({
  e2e: {
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
    env: {
      NEXT_PUBLIC_API_BASE_URL: 'http://localhost:3000',
    },
  },
});
