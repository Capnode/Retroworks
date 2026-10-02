# AGENTS.md

## Required Reading
- Read applicable `AGENTS.md` files from the workspace root to the edited path.
- Before making changes, consult the applicable requirements document, such as
	`REQUIREMENTS.md`, to avoid breaking existing requirements.
- When a requested code change conflicts with `REQUIREMENTS.md`, ask the user
	whether the requirement should change before editing, unless the user has
	explicitly requested that behavioral requirement change. In that case, update
	the requirement and implementation together.
- Verify every change against the project's requirements so existing
	requirements are not broken.
- Keep applicable agent instructions in context throughout the conversation.
- Any deviation from these instructions must be explicitly identified,
  justified, and accompanied by proposed corrective actions before work is
  considered complete.

## Document Responsibilities
- Keep reusable agent workflow and general C# and Avalonia guidance in
	`AGENTS.md`.
- Keep product, domain, architecture, safety, UI, and project-specific
	verification requirements in `REQUIREMENTS.md` or its equivalent.
- Keep `REQUIREMENTS.md`, implementation, and focused tests synchronized in
	both directions. A code change that adds, changes, or removes behavior
	covered by requirements must update those requirements in the same task. An
	explicit request to change behavioral requirements must update the
	implementation and focused tests in the same task; do not treat it as a
	documentation-only request or ask whether code should also change. Only keep a
	behavioral requirement change documentation-only when the user explicitly
	limits the request to documentation or identifies it as a future target. Do
	not rewrite requirements to legitimize unintended behavior; resolve
	unrequested divergence in the code or follow the existing conflict rule.
- Keep planned, deferred, and prioritized work in `BACKLOG.md` or its
	equivalent.
- Do not duplicate project requirements or backlog items in `AGENTS.md`.
- Use README files for descriptive project documentation, not as a substitute
	for requirements or agent instructions.

## Reverse Engineering
Läs först alla tillämpliga `AGENTS.md` och befintliga kravdokument.

Analysera sedan programmets faktiska kod och gör reverse engineering av dess observerbara beteende. Skapa eller komplettera `REQUIREMENTS.md` så att en utvecklare kan återskapa programmet i ett nytt repo utan tillgång till detta repo. Ändra inte programkoden.

Dokumentera:
- Användarflöden, UI-tillstånd, navigering, fel och återställning.
- Indata, utdata, filformat, persistensnycklar och standardvärden.
- Protokoll, byteformat, algoritmer, gränsvärden och externa beroenden som påverkar beteendet.
- Varianter eller profiler och exakt vad som skiljer dem åt.
- Konkreta testvektorer med indata och förväntade resultat, inklusive tomma, normala och angränsande gränsfall.

Härled varje krav från den kodväg som faktiskt bestämmer beteendet. Skilj tydligt på implementerad funktion, ofullständig funktion och sådant som inte går att fastställa. Hitta inte på önskat beteende. Hänvisa inte till originalrepots kod eller tester som enda specifikation: skriv ut informationen som behövs för att implementera och verifiera beteendet fristående.

Följ repots dokumentationsspråk och konventioner. Kontrollera exempel och beräknade värden mot koden, validera dokumentet och redovisa kvarvarande luckor som hindrar en fullständig återskapning.

## Documentation and Communication
- Keep documentation accurate and place requirements, implementation guidance,
	and planned work in their respective documents.
- Reuse information already in context; avoid redundant reads and lengthy
	status summaries unless they help the user make a decision.
- Keep work within the user's requested scope and report verified outcomes and
	unresolved failures clearly.
- When the user reports errors, state which ones were fixed and give the count
	when more than one was reported.
- Respond in the same language the user writes in, unless they request another
	language.
- Follow the repository's language and documentation conventions.

## Document Aliases
- `agents` means `AGENTS.md`.
- `requirements` means `REQUIREMENTS.md`.
- `backlog` means `BACKLOG.md`.

## General Rules
### Scope and Decisions
- Treat the user's complete request as the acceptance criteria. Before declaring
	work complete, verify every requested behavior against the owning code path,
	relevant reference implementations, and focused regression checks. Do not
	stop after fixing the first reported symptom or require the user to discover
	remaining deviations one at a time. If an intentional difference remains,
	state it explicitly in the final result.
- Execute the user's explicit instructions within their requested scope.
	Autonomy covers implementation choices needed to fulfill the request; it does
	not authorize unrelated changes, moving existing UI controls, adding
	unrequested restrictions, or expanding the task based on inferred intent.
- Do not execute the backlog unless the user explicitly says to do so.
- Keep project-specific commands, architecture, hardware, workflow, release
	information, and requirements in project documentation rather than in this
	general guidance.
- Recommend using a larger or stronger model when the task is ambiguous,
	spans multiple architectural boundaries, has high safety or production risk,
	requires difficult visual or geometric reasoning, or cannot be convincingly
	verified with focused checks. State the reason and the uncertainty explicitly.
- When a technical fact is unclear, research it independently in authoritative
	sources and inspect relevant local code or tests before asking the user; ask
	only when the evidence cannot resolve an ambiguity in intent or requirements.
- Preserve current APIs, conventions, and working behavior only when they are
	part of the current requirements or architecture. Otherwise, change or
	remove them when needed to implement the current request.
- Remove dead code made obsolete within the requested change scope. Do not keep
	obsolete implementations or compatibility shims unless they are explicitly
	required.
- Do not add or perform data migrations unless the user explicitly requests a
	migration. A storage-layout or schema change alone does not authorize moving,
	rewriting, or transforming existing persisted data. If an applicable
	requirement already mandates migration, stop and ask the user how to resolve
	the conflict before proceeding.
- Interpret acceptance criteria literally; do not add stricter thresholds,
	completeness rules, or safety gates that the user did not specify.
- For ambiguous behavioral or safety requirements, stop before changing the
	behavior and ask for clarification. Do not resolve ambiguity by choosing a
	stricter rule and presenting it as the requested behavior.
- When clarification is required, ask a concise multiple-choice question with
	clear, mutually distinguishable options, including the relevant alternatives
	and their consequences. Stop work that depends on the answer and wait for the
	user to choose; do not continue by guessing. An unavailable-user notice,
	empty selection, or tool-generated placeholder is not an answer or permission
	to proceed autonomously.
- Once the user explicitly selects an option or otherwise resolves the
	clarification, treat that as the controlling instruction and proceed within
	its scope. Do not reopen the settled question or impose additional conditions
	unless a new, material conflict or ambiguity arises.

### Evidence and Validation
- Treat model output and green tests as evidence, not proof. For visual issues,
	compare rendered pixels or measured screen coordinates when possible; for
	behavioral issues, use a reproducing test that exercises the real data path.
- For real machine or UI regressions, treat real logs, reproduction, and
	user-observed behavior as primary evidence; mocks are supporting evidence
	and must not be used to dismiss a real regression.
- Run the narrowest relevant verification after each substantive edit, then
	run the complete applicable checks before finishing.
- Builds should produce no warnings, and all applicable tests should pass.
- All test failures and warnings must be fixed, including failures in
	long-running tests; no test may be left failing or skipped to hide an
	unresolved problem.
- Before changing a working path, identify its exact current behavior, add or
	update a focused regression test for the requested behavior, and keep the
	first implementation change as small as possible.
- Convert every behavioral rule the user states into executable test cases
	before implementing it. Cover positive, rejecting-boundary, and nearest
	preserved cases; for numeric or count-based rules, test minimum, just-below,
	just-above, and empty cases where applicable.
- Do not treat a passing test suite as proof that the user's rule is covered:
	point to the specific test that encodes each stated rule, and add the test
	when no such test exists.
- For acceptance rules, write the complete boundary table explicitly in tests;
	do not conflate validation issues, completeness, and the user's acceptance
	condition. A stated minimum must not silently become an all-items-required
	condition.
- After a behavior change, verify both the requested case and preserved
	neighboring cases, including zero/empty, partial, and complete inputs when
	the behavior has acceptance thresholds.

### Cleanup and Behavior Preservation
- During cleanup, remove stale code and documentation, update Markdown, treat
	TODO items as unfinished work, inspect applicable logs and generated
	artifacts, fix discovered causes and deviations, and remove stale outputs.
- When investigating runtime behavior or hangs, inspect the application's
	documented log location.
- Ensure the application and tests are not running before or after cleanup.
- Cleanup is behavior-preserving work, not permission to restore older code or
	output. Before editing, record each confirmed behavior and optimization that
	must remain. For every such behavior, identify or add an executable
	regression check. Do not declare cleanup complete if any preserved behavior
	lacks a test or if an output, count, ordering, geometry, timing, or safety
	property regresses, even when the general test suite is green.
- Before replacing an implementation, separate the defect from the behavior it
	provides. Restore only the defective part; do not restore the whole previous
	implementation as a shortcut. If the distinction cannot be verified, stop
	and investigate before editing further.
- For optimizations, compare the pre-change and post-change behavior using the
	real data path and explicit metrics. A test must prove both the optimization
	and the preserved neighboring behavior. A passing geometry, compilation, or
	ordering test alone does not prove that a previously confirmed optimization
	still exists.
- Before declaring any task complete, review the diff against the recorded
	behavior list and state any unverified or intentionally changed behavior.

### Backlog Execution
- Remove completed and verified backlog items when the project maintains a
	backlog.
- When the user says to execute the backlog, implement, verify, and remove
	every current backlog item before declaring the work complete.
- When executing the backlog in autopilot mode, do not use the question tool.
	When choices need later user review, write numbered questions with labeled
	multiple-choice options directly in the chat, continue without waiting for a
	response, make conservative decisions and document assumptions, and stop
	only when required information cannot be safely inferred.

## C# and Logging
### Culture
- Use `InvariantCulture` for structured data, persistence, parsing, protocols,
	and file formats.
- Use `CurrentCulture` for UI display, `ToString`, validation, and converters.
- Preserve full precision in persisted values; apply significant-digit rounding
	only for UI display.

### Application Architecture and UI
- Keep shared domain state owned by its domain service; do not cache duplicate
	copies. ViewModels adapt state for UI binding, commands, and presentation.
- Keep reusable logic and data in services so other application interfaces can
	use them where applicable.
- Keep unit mathematics out of ViewModels and in services; expose UI labels
	through converters. Numeric input converters must return numeric values, not
	text.
- Unit changes must notify dependent scaled properties and preserve edit
	precision.
- Keep UI field definitions synchronized when controls are added or removed.
- Apply the application theme before creating the main window.
- Keep views and ViewModels up to date, including inactive ones; do not defer
	state, data, or layout updates solely because a view is inactive.
- User-facing controls should expose tooltips for discoverability.
- Optional operations should follow the same user-facing rules as other
	operations; avoid hidden special cases and asymmetric controls.
- Settings and file writes should tolerate transient access failures without
	corrupting existing data.
- Tests should use isolated settings and temporary files rather than modifying
	the default persistent user settings.
- UI behavior that depends on the UI framework must have appropriate headless
	or UI regression coverage, including affected bindings, formatting, and
	unit-dependent behavior.

### Log Volume
- Prefer focused logging of significant events; avoid high-frequency telemetry
	by default.
- Use ERROR for severe errors without recovery and WARNING for errors with a
	recovery path. Use INFO for general user information, DEBUG for low-frequency
	debugging, and TRACE for high-frequency debugging.

## Bug Fixing and Avalonia Tests
- Establish a test baseline before editing when practical, but always fix
	every discovered failing or incomplete test and every test warning. This
	includes issues that existed before the current change; never dismiss them
	as pre-existing or defer them as unrelated.
- Reproduce a bug with a focused test when practical.
- Fix the root cause rather than masking the symptom.
- Every bug fix must have a verifying test. Rerun the reproducing test after
	the fix; if reproducing the exact failure was not practical, add focused
	regression coverage for the reported behavior.
- Fix all errors and warnings found immediately; do not defer them or classify
	them as pre-existing to avoid fixing them.
- UI bug fixes must have focused regression tests, and all UI tests must run
	headlessly.
- Tests that exercise Avalonia-dependent logic must use the documented
	Avalonia headless test harness.
