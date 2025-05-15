import os
import re

EXCLUDED_DIRS = {'.git', 'node_modules', '.next', 'dist', '__pycache__'}
TARGET_EXTENSIONS = {'.cs'}
ENV_FILE = '.env'

HARD_CODED_PATTERNS = {
    'URLs': r'http[s]?://[^\s"\']+',
    'IP Addresss': r'\b(?:\d{1,3}\.){3}\d{1,3}\b',
    'Ports': r'(?<![\w:])(?<!\d)(\d{2,5})(?!\d)(?!\s*(?:-|to|–|/)\s*\d+)',
    'Passwords': r'(?i)\b(password|pwd|pass|secret|token|auth)[\s:=]+[\'"]?[^\'"\s]+[\'"]?',
    'Token Keywords': r'(?i)\b(auth_token|access_token|Authorization|Bearer|token)\b',
    'JWT Keywords': r'(?i)\bjwt\b',
    'API Keys': r'(?i)\b(api[-_]?key)[\s:=]+[\'"]?[^\'"\s]+[\'"]?',
    'Connection Strings': r'(?i)\b(Data Source|Server=|uid=|user=|Username=|LoginUser|connectionString)\b',
    'Common Hosts': r'(?i)\blocalhost|127\.0\.0\.1|192\.168\.\d+\.\d+\b',
}

matches_summary = []

def search_hardcoded_values(file_path):
    """Durchsuche die Datei nach hardcodierten Werten."""
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.readlines()

    file_matches = []
    for label, pattern in HARD_CODED_PATTERNS.items():
        for line_num, line in enumerate(content, start=1):
            matches = re.findall(pattern, line)
            if matches:
                unique_matches = sorted(set(matches))
                file_matches.append(f"  🔹 {label}s: {', '.join(unique_matches[:5])}" + (" ..." if len(unique_matches) > 5 else ""))
                file_matches.append(f"    Zeile {line_num}: {line.strip()}")  # Füge die Zeile mit dem Treffer hinzu

    if file_matches:
        relative_path = os.path.relpath(file_path)
        matches_summary.append(f"\n📄 Datei: {relative_path}")
        matches_summary.extend(file_matches)

def scan_project_for_hardcoded_values(root_dir):
    """Scanne das Projektverzeichnis nach C#-Dateien."""
    for dirpath, dirnames, filenames in os.walk(root_dir):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDED_DIRS]
        for filename in filenames:
            _, ext = os.path.splitext(filename)
            if ext in TARGET_EXTENSIONS:
                full_path = os.path.join(dirpath, filename)
                search_hardcoded_values(full_path)

def create_env_file():
    """Erstelle die .env Datei und schreibe die gefundenen Variablen hinein."""
    with open(ENV_FILE, 'w', encoding='utf-8') as env_file:
        for label, pattern in HARD_CODED_PATTERNS.items():
            found_values = set()
            for file in matches_summary:
                matches = re.findall(pattern, file)
                found_values.update(matches)
            for value in found_values:
                env_file.write(f'{label.upper().replace(" ", "_")}_{label}: {value}\n')

def update_code_with_env():
    """Aktualisiere den Code, um Umgebungsvariablen zu verwenden."""
    # Extrahiere nur die Dateipfade aus matches_summary
    file_paths = [line.split(":")[1].strip() for line in matches_summary if line.startswith("📄 Datei")]

    for file in file_paths:
        with open(file, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()

        for label in HARD_CODED_PATTERNS.keys():
            pattern = re.escape(label)
            content = re.sub(pattern, f"Environment.GetEnvironmentVariable('{label.upper().replace(' ', '_')}_{label}')", content)

        with open(file, 'w', encoding='utf-8') as f:
            f.write(content)

if __name__ == "__main__":
    current_dir = os.getcwd()
    print(f"🚀 Durchsuche Projekt unter: {current_dir}")
    scan_project_for_hardcoded_values(current_dir)

    # Erstelle die .env Datei
    create_env_file()
    print(f"✅ Ergebnisse gespeichert in '{ENV_FILE}'")

    # Aktualisiere den Code, um Umgebungsvariablen zu verwenden
    update_code_with_env()
    print("✅ Code wurde aktualisiert.")
