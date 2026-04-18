#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_BIN="${DOTNET:-dotnet}"
VERSION="$(python - <<'PY' "$ROOT_DIR/AATool/Properties/AssemblyInfo.cs"
import re
import sys
from pathlib import Path

text = Path(sys.argv[1]).read_text()
matches = re.findall(r'^\[assembly:\s*AssemblyVersion\("([^"]+)"\)\]$', text, re.MULTILINE)
if not matches:
    raise SystemExit('Could not determine AssemblyVersion')
print(matches[-1])
PY
)"
RID="linux-x64"
PUBLISH_DIR="$ROOT_DIR/dist/publish/$RID"
BUNDLE_NAME="AATool-arch-linux-$VERSION-$RID"
BUNDLE_DIR="$ROOT_DIR/dist/$BUNDLE_NAME"
APP_DIR="$BUNDLE_DIR/app"
ARCHIVE_PATH="$ROOT_DIR/dist/$BUNDLE_NAME.tar.gz"

rm -rf "$PUBLISH_DIR" "$BUNDLE_DIR" "$ARCHIVE_PATH"
mkdir -p "$PUBLISH_DIR" "$APP_DIR"

"$DOTNET_BIN" publish "$ROOT_DIR/AATool/AATool.csproj" \
  -c Release \
  -f net8.0 \
  -r "$RID" \
  --self-contained true \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false \
  -o "$PUBLISH_DIR"

rm -rf "$PUBLISH_DIR/config" "$PUBLISH_DIR/logs"
mkdir -p "$PUBLISH_DIR/config" "$PUBLISH_DIR/logs"

if [[ ! -d "$PUBLISH_DIR/config.defaults" ]]; then
  cp -r "$ROOT_DIR/config.defaults" "$PUBLISH_DIR/config.defaults"
fi

cat > "$BUNDLE_DIR/run-aatool.sh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
APP_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$APP_DIR/app"
exec "$APP_DIR/app/AATool" "$@"
EOF
chmod +x "$BUNDLE_DIR/run-aatool.sh"

cp -r "$PUBLISH_DIR/." "$APP_DIR/"
cp "$ROOT_DIR/README.md" "$BUNDLE_DIR/README.md"
cp "$ROOT_DIR/LICENSE.md" "$BUNDLE_DIR/LICENSE.md"
cp "$ROOT_DIR/info/linux-arch.md" "$BUNDLE_DIR/README-ARCH-LINUX.md"

tar -C "$ROOT_DIR/dist" -czf "$ARCHIVE_PATH" "$BUNDLE_NAME"

printf 'Created %s\n' "$ARCHIVE_PATH"
