import os
import re

EXCLUDED_DIRS = {'.git', 'node_modules', '.next', 'dist', '__pycache__'}
INCLUDED_EXTENSIONS = {'.cs', '.js', '.ts', '.tsx', '.vue', '.html', '.css'}

# Verbesserte Regex-Patterns
PATTERNS = {
    'class': r'\bclass\s+(\w+)',
    'function': r'\bfunction\s+(\w+)|(\w+)\s*=\s*function|\b(\w+)\s*=\s*\(.*?\)\s*=>',
    'method': r'def\s+(\w+)|public\s+[\w<>]+\s+(\w+)\s*\(|(\w+)\s*\([^)]*\)\s*{',
    'variable': r'\b(const|let|var|int|string|bool|float)\s+(\w+)',
    'component': r'<template>|React\.Component|defineComponent|export\s+default|setup\(\)',
}

output_lines = []

def extract_info(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

    result = [f"\n📄 Datei: {filepath}"]

    for key, pattern in PATTERNS.items():
        matches = re.findall(pattern, content)
        flat_matches = {m for tup in matches for m in tup if m}
        if flat_matches:
            result.append(f"  🔹 {key.capitalize()}s: {', '.join(sorted(flat_matches))}")

    return '\n'.join(result)

def scan_directory(root_dir):
    for dirpath, dirnames, filenames in os.walk(root_dir):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDED_DIRS]
        for filename in filenames:
            _, ext = os.path.splitext(filename)
            if ext in INCLUDED_EXTENSIONS:
                full_path = os.path.join(dirpath, filename)
                info = extract_info(full_path)
                output_lines.append(info)

if __name__ == "__main__":
    current_dir = os.getcwd()
    print(f"🚀 Durchsuche Projekt unter: {current_dir}")
    scan_directory(current_dir)
    with open("project_overview.txt", "w", encoding="utf-8") as f:
        f.write('\n'.join(output_lines))
    print("✅ Übersicht gespeichert in 'project_overview.txt'")
