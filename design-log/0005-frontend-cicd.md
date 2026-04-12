# Design Log #0005 - CI/CD for Frontend

## Background
We need an automated pipeline for the Vue.js frontend to ensure code quality through linting and testing, and to automate deployment to Vercel.

## Problem
Currently, linting, testing, and deployment are manual processes. This increases the risk of regressions and makes the deployment process slower and more error-prone.

## Questions and Answers
*   **Q: What linting tool should we use?**
    *   A: **ESLint** with the official Vue plugin and TypeScript support.
*   **Q: How will we deploy to Vercel?**
    *   A: We will use the **Vercel CLI** within GitHub Actions. This requires three secrets: `VERCEL_TOKEN`, `VERCEL_ORG_ID`, and `VERCEL_PROJECT_ID`.
*   **Q: Should the pipeline run on every push?**
    *   A: ✅ Yes, but deployment will only happen on the `main` or `feature/sprint-3-tasks` branch (or whatever the primary branch is).

## Design

### 1. Linting & Type Checking
*   Step 1: `npm install`
*   Step 2: `npm run lint` (ESLint)
*   Step 3: `npm run type-check` (vue-tsc)

### 2. Testing
*   Step 4: `npm run test` (Vitest)

### 3. Deployment
*   Step 5: Install Vercel CLI.
*   Step 6: Pull Vercel environment information.
*   Step 7: Build and deploy.

## Implementation Plan

### Phase 1: Linting Setup
1.  Install `@vue/eslint-config-typescript`, `eslint`, and `eslint-plugin-vue`.
2.  Create `.eslintrc.json`.
3.  Add `lint` and `type-check` scripts to `package.json`.

### Phase 2: GitHub Actions Workflow
1.  Create `.github/workflows/frontend-cd.yml`.
2.  Define jobs: `lint-and-test` and `deploy`.

## Examples

### ✅ GitHub Actions Trigger
```yaml
on:
  push:
    branches: [ main, feature/sprint-3-tasks ]
    paths:
      - 'frontend/**'
```

## Trade-offs
*   **CI Speed**: Adding linting and testing increases the time it takes for a PR to be ready, but it drastically improves code reliability.

## Implementation Results
*   **ESLint Setup**: Installed `eslint`, `eslint-plugin-vue`, and `@vue/eslint-config-typescript`. Created `.eslintrc.json` with standard Vue 3 rules.
*   **Scripts**: Added `lint`, `type-check`, and updated `test` to `vitest run` in `package.json`.
*   **GitHub Action**: Created `.github/workflows/frontend-cd.yml` with two jobs:
    1.  `lint-and-test`: Performs linting, type checking, and unit testing.
    2.  `deploy`: Deploys the application to Vercel (triggered on push to `main` or `feature/sprint-3-tasks`).
*   **Vercel Integration**: Configured the workflow to use the Vercel CLI for building and deploying prebuilt artifacts.

## Summary of Deviations from Original Design
*   **Added npm-run-all**: Added `npm-run-all` to support the `run-p` command in the `build` script, ensuring a cleaner build process.
*   **Explicit vitest run**: Changed the test command from `vitest` to `vitest run` to ensure the CI job terminates correctly.
