#!/usr/bin/env bash
set -euo pipefail

# Run from repository root. Requires pandoc and a LaTeX engine (e.g., texlive) installed.
OUT_DIR="docs/security"
OUT_PDF="$OUT_DIR/Justine_Security_Design.pdf"

# Ensure files exist
mkdir -p "$OUT_DIR"
files=(
  "$OUT_DIR/overview.md"
  "$OUT_DIR/architecture.md"
  "$OUT_DIR/threat-model.md"
  "$OUT_DIR/data-protection.md"
  "$OUT_DIR/identity-access.md"
  "$OUT_DIR/network.md"
  "$OUT_DIR/operations.md"
  "$OUT_DIR/roadmap.md"
)

for f in "${files[@]}"; do
  if [ ! -f "$f" ]; then
    echo "Missing $f"
    exit 1
  fi
done

# Combine and render to PDF
pandoc "${files[@]}" -s -o "$OUT_PDF" --metadata title="Justine Security Design" --pdf-engine=xelatex

echo "PDF generated at: $OUT_PDF"