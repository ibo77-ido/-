# P8-03D Bridge Execution

## Recommended Implementation Order

1. Create the session and runtime mode types.
2. Add the bridge manager and input lock.
3. Add the bridge canvas host and module adapter.
4. Add the minimal Phase3 gate.
5. Wire Result exit into bridge completion.
6. Bind Phase6 `E` interaction to the bridge.
7. Validate the first runtime chain.

## First Validation Slice

Start with one short path only:

- `Order -> Shape`

This first slice should prove:

- the bridge can enter a module
- the bridge can block auto-advance
- the bridge can release control cleanly

## Go / No-Go

### Go

- One area maps to one module.
- Completion returns to Phase6.
- No repeated UI open.
- No input lock dead-end.
- No Phase3 auto-chain leak.

### No-Go

- Session reuse across different areas.
- Duplicate managers in the scene.
- Result exit firing twice.
- Any Phase3 dependency on Phase6.
