# P8-00 Phase8 Overview

## Goal

Phase8 connects the Phase6 world layer to the Phase3 gameplay modules through a runtime bridge.

## Target Result

- Phase6 keeps world movement, region detection, and `E` interaction.
- Bridge owns session, input lock, UI host, and lifecycle control.
- Phase3 keeps Order, Shape, Glaze, Firing, and Result logic.
- Phase3 auto-advance is blocked in bridge mode.
- Player must return to Phase6 before entering the next region.

## Non-Goals

- Do not merge Phase3 and Phase6 into one gameplay loop.
- Do not make Phase3 depend on Phase6.
- Do not add a large framework before the minimal runtime bridge works.

## Success Criteria

- One Phase6 area can open one Phase3 module.
- The module works normally after entry.
- Completion returns control to Phase6.
- No automatic chained advance leaks through the bridge.
