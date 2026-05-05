# Frontend — React / TypeScript

See root `CLAUDE.md` for project vocabulary and cross-cutting rules.

## Tech choices (decided)
- **Framework**: React 19 with TypeScript (strict mode)
- **Build tool**: Vite
- **Routing**: React Router v7 (file-based routing via `src/routes/`)
- **State**: Zustand for global client state; TanStack Query for server state
- **Styling**: Tailwind CSS v4
- **Forms**: React Hook Form + Zod for validation
- **Testing**: Vitest + React Testing Library

## Project structure (target — not yet created)
```
frontend/
  src/
    routes/              # Page-level route components (atomic: Pages)
    components/
      atoms/             # Smallest units: Button, Input, Label, Badge, Icon
      molecules/         # Atoms composed: FormField, StatusBadge, SearchBar
      organisms/         # Complex sections: JobCard, ApplicationTable, NavBar
      templates/         # Layout shells with slot props, no real data
    features/            # Feature-sliced modules: jobs/, applications/, contacts/
      jobs/
        api.ts           # TanStack Query hooks for this feature
        types.ts         # Zod schemas + inferred TS types
        hooks.ts         # Non-query feature hooks (e.g. useJobFilters)
    stores/              # Zustand stores (UI state only — not server data)
    lib/                 # Pure utilities, API client setup
  public/
  index.html
```

## Atomic design levels
- **Atoms** (`components/atoms/`): single-purpose, no business logic, no data fetching. Styled HTML wrappers — Button, Input, Label, Badge, Spinner.
- **Molecules** (`components/molecules/`): compose atoms into a reusable unit — FormField (Label + Input + error), StatusBadge (Badge + icon). Still no data fetching.
- **Organisms** (`components/organisms/`): self-contained UI sections that may accept complex props but don't fetch their own data — JobCard, ApplicationTable, ContactList. Wired to real data by feature components or routes.
- **Templates** (`components/templates/`): page layout shells. Accept slot props (`header`, `sidebar`, `children`). No business logic.
- **Pages** (`routes/`): React Router route components. Compose templates + organisms + TanStack Query hooks. This is where data fetching happens.

## Tauri shell
- Tauri config lives in `src-tauri/` — do not put React concerns there.
- `src-tauri/tauri.conf.json` controls window config, sidecar registration, and permissions.
- The .NET API is registered as a Tauri sidecar — Tauri spawns/kills it automatically.
- React communicates with the API via `localhost` as normal; Tauri is transparent to React code.

```powershell
# Dev (Tauri window + Vite hot reload)
cargo tauri dev

# Production build (bundles React + sidecar into installer)
cargo tauri build
```

## Essential commands
```powershell
# Install deps (first time)
npm install

# Dev server (Vite only — no Tauri window)
npm run dev

# Type check
npm run typecheck   # or: npx tsc --noEmit

# Run tests
npm test            # watch mode
npm run test:run    # single pass (CI)

# Build for production
npm run build
```

## Code conventions
- **No `any`** — use `unknown` and narrow, or write the type. `as` casts require a comment explaining why.
- **Co-locate by feature (data layer only)** — API hooks and Zod types live under `src/features/<feature>/`. UI components live in `components/` at the appropriate atomic level, not inside `features/`.
- **TanStack Query owns server state** — never copy API responses into Zustand. Zustand is for UI-only state (selected row, open modal, etc.).
- **Zod first** — define the schema in `types.ts`, infer the TS type from it with `z.infer<>`. Don't write separate interface and schema.
- **Named exports only** — no default exports except for route components (required by React Router).
- **No prop drilling past two levels** — use context or a store instead. Pick context for subtree-scoped state (e.g. open accordion); pick Zustand for state shared across unrelated parts of the tree (e.g. selected job ID, modal open state).

## Component rules
- One component per file, filename matches component name.
- Atoms and molecules: pure presentational — no data-fetching hooks, no store access. Local UI hooks (`useState`, `useRef`, `useId`) are fine.
- Organisms: may receive complex/domain props but must not fetch their own data.
- Templates: layout only — no domain props, only slot props and layout-level state.
- Data fetching lives only in route components (`routes/`) or feature `api.ts` hooks consumed by routes.
- Always type children as `React.ReactNode` when a component accepts them.
- When unsure which level a component belongs at, ask: "can I render this with fake data in isolation?" If yes, it's an atom/molecule/organism. If it needs real data to make sense, it belongs in a route.
- Don't skip levels — a molecule must not compose organisms; an organism must not nest templates. If something naturally requires composing across levels, promote it up: make it an organism.
- Forms are organisms — a form composes molecule fields (FormField) and atom controls (Button). Forms live in `components/organisms/` unless they are feature-specific and one-off, in which case they live in the route file directly.
- Every component that can show loading or error state must handle it explicitly — no silent empty renders. Atoms/molecules receive `isLoading`/`error` props if needed; organisms use Suspense boundaries or inline guards; routes wrap organisms in `<ErrorBoundary>`.
- Atoms must forward `ref` and spread `...rest` props so consumers can attach `aria-*`, `data-*`, and event handlers without modifying the atom. Use `React.forwardRef` for all atom input/button/interactive elements.
- Do not apply `React.memo` preemptively. Only add it when a profiler session shows unnecessary re-renders and the fix is not a better selector or restructured props.

## Styling conventions
- Use `cn()` from `src/lib/cn.ts` (wraps `clsx` + `tailwind-merge`) for all conditional or merged class strings. Never use template literals or string concatenation for Tailwind classes. `cn.ts`: `export function cn(...inputs: ClassValue[]) { return twMerge(clsx(inputs)); }`
- All design tokens (colors, spacing, typography) defined in `src/styles/theme.css` using Tailwind v4's `@theme` directive.
- No raw color values in components — no hex, no `oklch(...)`, no `rgb(...)`. Always use semantic token classes (`bg-brand-primary`, `text-status-rejected`).
- Token names are semantic/purpose-based, not value-based: `color-status-applied` not `color-blue-500`.
- No BEM — Tailwind utility classes only. No hand-written CSS class names in components.
- Changing a theme color means editing `theme.css` only — if you find yourself updating colors in multiple files, stop and add a token instead.

## Zustand stores
- One store per domain concern: `useUiStore.ts` (cross-cutting UI), `useJobStore.ts` (job selection/filter state), etc. No single god store.
- Store shape: `{ state fields } & { actions }` — actions defined inside `create()`, not as separate hooks.
- No derived/computed values in store state — derive in selectors at the call site with `useStore(s => s.x)`.
- Do not use `React.memo` to compensate for a store that updates too broadly — fix the store selector instead.

## API client
- Base URL read from `import.meta.env.VITE_API_URL` (set in `.env.local`, gitignored).
- All fetch calls go through `src/lib/apiClient.ts` — never raw `fetch` in components.
- API errors are thrown as typed `ApiError` objects so TanStack Query error states are consistent. Shape:
  ```ts
  interface ApiError {
    status: number;      // HTTP status code
    code: string;        // machine-readable error key from backend
    message: string;     // human-readable fallback
  }
  ```

## Testing conventions
- Test files co-located with source: `Button.tsx` → `Button.test.tsx` in same directory.
- **Atoms**: test rendered output and prop variations. No mocking needed.
- **Molecules**: test composed behavior (e.g. FormField shows error message when `error` prop set). No mocking needed.
- **Organisms**: test user interactions (click, submit, keyboard). Mock TanStack Query hooks or pass data via props — never mock the API client directly.
- **Routes**: test full user flows with `MemoryRouter`. Mock `api.ts` hooks, not fetch.
- **Hooks** (`features/*/hooks.ts`): test with `renderHook` from React Testing Library.
- Do not mock Zustand stores — set initial state via the store's API in `beforeEach`.
- Do not test implementation details (internal state, private functions). Test behavior a user can observe.
- Test file must exist for every non-trivial atom, molecule, organism, and custom hook. Trivial = pure pass-through with no logic (e.g. a single-element wrapper with one className).

## Future CLAUDE.md splits
As directories grow, extract these sections into subdirectory CLAUDE.md files to avoid loading all rules on every task:
- `src/components/CLAUDE.md` ← Atomic design levels + Component rules
- `src/stores/CLAUDE.md` ← Zustand stores
- `src/styles/CLAUDE.md` ← Styling conventions
Keep at this file: Tech choices, Project structure, Essential commands, Code conventions, API client, Testing conventions, What NOT to do.

## What NOT to do
- Do not use `useEffect` to fetch data — use TanStack Query.
- Do not store server data in Zustand.
- Do not import from `src/features/X` inside `src/features/Y` — features must not depend on each other.
- Do not use index files (`index.ts`) to re-export everything — they obscure the actual import path.
