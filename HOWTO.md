# How To Initialize and Execute This Project

> This guide covers the exact steps to initialize and execute the Cargo Ship Monitoring platform using compartmentalized agent-driven development. Follow this to minimize context window usage and token consumption.

---

## Prerequisites

- Claude Code installed and available as `claude` in your PATH
- .NET 8 SDK installed (`dotnet --version` should show 8.x)
- Docker and docker-compose installed (for Milestone 10)
- Git initialized in the project folder (or run `git init` before Step 1)

---

## Step 0: Understand the Project Structure

The plan is compartmentalized into 11 milestones, each in its own folder:

```
docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/
├── .agent-context              # Rules every agent must follow (42 lines)
├── MANIFEST.md                 # Build state tracker (53 lines)
├── COMPLETION_TEMPLATE.md      # Template for post-work documentation
├── README.md                   # Plan overview with architecture diagram
├── 01-solution-scaffolding/
│   ├── README.md               # Decisions + Required Context
│   └── tasklist.md             # Exact implementation steps
├── 02-priority-event-queue/
│   ├── README.md
│   └── tasklist.md
... (milestones 03-11)
```

Each milestone has:
- **README.md**: What it builds, decisions made, required context from prior work, dependencies, acceptance criteria
- **tasklist.md**: Concrete implementation steps with commands and code
- **COMPLETION.md**: (created after finishing) What was actually built, deviations, test results

---

## Step 1: Open Claude Code in the Project Folder

```bash
cd /home/suzuki/code/alina
claude
```

You now have **one** Claude Code session. You are the orchestrator. All work happens from here.

---

## Step 2: Read Only the Essential Files

Inside your Claude session, read exactly these files in order. Nothing else.

1. **MANIFEST.md** — Tells you what is done vs pending
2. **.agent-context** — Rules every subagent must follow
3. **01-solution-scaffolding/README.md** — What Milestone 1 builds
4. **01-solution-scaffolding/tasklist.md** — Exact commands to execute

**Total: ~350 lines.** Do not read the top-level README.md, any other milestone, or the design spec.

---

## Step 3: Execute Milestone 1 Yourself

Milestone 1 is scaffolding. Execute it directly in your session (do not delegate). The tasklist has the exact commands:

```bash
# From the tasklist — run these in your terminal
dotnet new sln -n CargoShipMonitoring
dotnet new classlib -n CargoShipMonitoring.Shared -o src/Shared
# ... (rest of commands from tasklist)
```

After executing, verify:

```bash
dotnet build
dotnet test tests/ShipEdge.Tests --filter "SharedEventTests"
```

Expected: Build succeeds, tests pass.

---

## Step 4: Create COMPLETION.md for Milestone 1

Copy the template from `COMPLETION_TEMPLATE.md` into `docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/01-solution-scaffolding/COMPLETION.md` and fill it out:

- What was built
- Files created
- Any deviations
- Test results
- Commit hash

---

## Step 5: Update MANIFEST.md

Edit `MANIFEST.md`:
- Change Milestone 01 status to `COMPLETE`
- Add the commit hash
- Link to `01-solution-scaffolding/COMPLETION.md`

---

## Step 6: Dispatch Parallel Agents for Independent Milestones

Now Milestone 1 is done. Two independent groups unlock:

| Group A (Ship Edge) | Group B (Fleet Cloud) |
|---|---|
| 02 Priority Event Queue | 07 Fleet Cloud API |
| 03 Rules Engine | 08 Command Service |
| 04 Circuit Breaker | |
| 05 Satellite Gateway | |

Dispatch all 6 agents in **one message** using the `Agent` tool. Each agent works independently and reports back.

### Agent Prompt Template

Use this exact prompt for each agent (customize the milestone number and name):

```
You are implementing Milestone [N]: [Name] for the Cargo Ship Monitoring platform.

## Rules (follow exactly)
1. Read ONLY the files listed below. Do NOT read any other files.
2. Do NOT read the design spec unless your README explicitly references it.
3. Do NOT read tasklists of other milestones.
4. After completing work, create COMPLETION.md using the template at:
   docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/COMPLETION_TEMPLATE.md
5. Commit after each task with a descriptive message.
6. Update MANIFEST.md: set your milestone status to COMPLETE, add commit hash, link to COMPLETION.md.

## Files You Must Read
1. docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/.agent-context
2. docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/0[N]-[name]/README.md
3. docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/0[N]-[name]/tasklist.md
4. [List dependency COMPLETION.md files from README's "Required Context" section]

## Files You Must NOT Read
- Any README.md or tasklist.md from other milestones
- The design spec at docs/superpowers/specs/2026-05-19-cargo-ship-monitoring-design.md

## Your Task
Implement the milestone following the tasklist exactly.

## Acceptance Criteria You Must Verify
Run the test commands from the tasklist and confirm they pass.

## Project Context
- .NET 8, solution file: CargoShipMonitoring.sln
- Shared library: src/Shared/
- Your code goes in the paths specified in the tasklist
```

### Example: Dispatching Milestone 2

```
Agent({
  description: "Milestone 2: Priority Event Queue",
  prompt: "You are implementing Milestone 2: Priority Event Queue..."
})
```

### Dispatch All 6 in Parallel

Send one message with 6 Agent tool calls:

```
Agent({ description: "Milestone 2", prompt: "..." })
Agent({ description: "Milestone 3", prompt: "..." })
Agent({ description: "Milestone 4", prompt: "..." })
Agent({ description: "Milestone 5", prompt: "..." })
Agent({ description: "Milestone 7", prompt: "..." })
Agent({ description: "Milestone 8", prompt: "..." })
```

Each agent works in the background. You get notified as each completes.

---

## Step 7: Poll MANIFEST.md for Progress

To check progress, read only one file:

```
Read docs/superpowers/plans/2026-05-19-cargo-ship-monitoring/MANIFEST.md
```

This is 53 lines. It tells you everything: which milestones are complete, which are pending, what's active.

**Do NOT** re-read tasklists or COMPLETION.md files just to check progress.

---

## Step 8: Dispatch Milestone 6 (Integrator)

When Group A (02-05) are all `COMPLETE`, dispatch Milestone 6:

```
Agent({
  description: "Milestone 6: Ship Edge Worker",
  prompt: "You are implementing Milestone 6..."
})
```

This agent needs context from Milestones 01-05. Its README lists exactly which COMPLETION.md files to read.

---

## Step 9: Dispatch Final Milestones (09-11)

When all prior work is complete, dispatch in parallel:

```
Agent({ description: "Milestone 9: Simulators", prompt: "..." })
Agent({ description: "Milestone 10: Docker Compose", prompt: "..." })
Agent({ description: "Milestone 11: Integration Tests", prompt: "..." })
```

---

## Token Efficiency Rules

| Rule | Why |
|------|-----|
| Read only MANIFEST.md to check progress | 53 lines vs 3,000+ for full exploration |
| Read only your milestone's files | ~400 lines vs 1,200+ for reading everything |
| Read only COMPLETION.md from dependencies | ~30 lines vs full README + tasklist |
| Do NOT read tasklist.md for completed milestones | Contains stale implementation steps |
| Do NOT read the design spec | Your milestone README has everything you need |
| Always use `dotnet test --filter` | Avoids running unrelated tests |
| Dispatch agents with exact file paths | Prevents agents from exploratory reading |

---

## Anti-Patterns (What NOT To Do)

| Anti-Pattern | Cost | Solution |
|-------------|------|----------|
| Reading all 11 READMEs upfront | ~1,000 lines of stale context | Read only your milestone |
| Re-reading files to "verify" state | Wasted tokens | Trust MANIFEST.md |
| "Explore the plan directory and figure it out" | Agent reads everything | Give exact file paths in prompt |
| Keeping completed milestone details in session | Clutters context | Summarize in MANIFEST, drop detail |
| Running `dotnet test` without `--filter` | Runs all tests | Filter to your test class |
| Dispatching agents without `.agent-context` rules | Agent may read irrelevant files | Include rules in every prompt |

---

## Context Budget Per Agent

| What | Lines |
|------|-------|
| `.agent-context` | 42 |
| Milestone README | ~140 |
| Milestone tasklist | ~200 |
| Dependency COMPLETION.md files | ~30 each |
| **Total** | **~400-500** |

Compare to naive approach: ~3,000 lines. **Savings: ~85%**.

---

## Troubleshooting

### Agent asks about a file not in its reading list
Answer: "Do not read that file. Follow the file list in your prompt exactly."

### Agent wants to read the design spec
Answer: "Your milestone README contains everything you need. Do not read the design spec."

### Agent reports DONE_WITH_CONCERNS
Read the concerns. If about correctness, address before proceeding. If observations, note and continue.

### Agent reports BLOCKED
Assess: Is it missing context? Provide it. Is the task too large? Break it down. Is the plan wrong? Escalate to yourself.

### Two agents conflict on filesystem
Use `EnterWorktree` or `git worktree add` to give each agent an isolated branch. Merge after review.

---

## Summary Workflow

```
1. cd /home/suzuki/code/alina && claude
2. Read MANIFEST.md + .agent-context + 01/README.md + 01/tasklist.md
3. Execute Milestone 1 yourself (scaffolding)
4. Create 01/COMPLETION.md, update MANIFEST.md
5. Dispatch 6 parallel agents for 02-05 and 07-08
6. Poll MANIFEST.md for progress
7. Dispatch Milestone 6 when 02-05 are done
8. Dispatch 09-11 when everything else is done
9. Final review and merge
```

---

## Files You Need to Know

| File | Purpose | When to Read |
|------|---------|-------------|
| `MANIFEST.md` | Build state tracker | Every time you check progress |
| `.agent-context` | Rules for all agents | Once at start, include in every agent prompt |
| `COMPLETION_TEMPLATE.md` | Template for post-work docs | When creating COMPLETION.md |
| `[NN]-[name]/README.md` | Decisions + required context | When implementing that milestone |
| `[NN]-[name]/tasklist.md` | Implementation steps | When implementing that milestone |
| `[NN]-[name]/COMPLETION.md` | What was actually built | When integrating with that milestone |
