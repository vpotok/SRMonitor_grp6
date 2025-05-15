import os
import re

EXCLUDED_DIRS = {'.git', 'node_modules', '.next', 'dist', '__pycache__'}
TARGET_EXTENSIONS = {'.cs'}

HARD_CODED_PATTERNS = {
    'URL': r'http[s]?://[^\s"\']+',
    'IP Address': r'\b(?:\d{1,3}\.){3}\d{1,3}\b',
    'Port': r'(?<![\w:])(?<!\d)(\d{2,5})(?!\d)(?!\s*(?:-|to|–|/)\s*\d+)',
    'Password': r'(?i)\b(password|pwd|pass|secret|token|auth)[\s:=]+[\'"]?[^\'"\s]+[\'"]?',
    'API Key': r'(?i)\b(api[-_]?key)[\s:=]+[\'"]?[^\'"\s]+[\'"]?',
    'JWT': r'eyJ[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+',
    'JWT Keyword': r'(?i)\bjwt\b',
    'Token Keyword': r'(?i)\b(auth_token|access_token|Authorization|Bearer|token)\b',
    'Secret Key': r'(?i)\b(secret_key|private_key|public_key|ssh_key)\b',
    'Client Info': r'(?i)\b(client_secret|client_id)\b',
    'DB Credentials': r'(?i)\b(db_pass|db_password|root_password|ftp_password)\b',
    'Connection Strings': r'(?i)\b(Data Source|Server=|uid=|user=|Username=|LoginUser|connectionString)\b',
    'Common Hosts': r'(?i)\blocalhost|127\.0\.0\.1|192\.168\.\d+\.\d+\b',
    'Base URLs': r'(?i)\b(base_url|REDIS_URL|KAFKA_BROKER|mongodb://|redis://|ftp://)\b',
    'AWS/Azure': r'(?i)\b(aws_secret_access_key|azure_storage_key|s3_bucket)\b',
    'SMTP/Email': r'(?i)\b(smtp|email_host_password)\b',
    'Debug': r'(?i)\bDEBUG=true\b',
    'Simulate': r'(?i)\bsimulate_alarm\b',
    'Private Key Block': r'(?i)-----BEGIN PRIVATE KEY-----',
    # Weitere Muster für andere hardcodierte Daten
}

matches_summary = []

def search_hardcoded_values(file_path):
    with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.readlines()

    file_matches = []
    for label, pattern in HARD_CODED_PATTERNS.items():
        for line_num, line in enumerate(content, start=1):
            matches = re.findall(pattern, line)
            if matches:
                unique_matches = sorted(set(matches))
                file_matches.append(f"  🔹 {label}s: {', '.join(unique_matches[:5])}" + (" ..." if len(unique_matches) > 5 else ""))
                file_matches.append(f"    Zeile {line_num}: {line.strip()}")  # Adding the line with the match

    if file_matches:
        relative_path = os.path.relpath(file_path)
        matches_summary.append(f"\n📄 Datei: {relative_path}")
        matches_summary.extend(file_matches)

def scan_project_for_hardcoded_values(root_dir):
    for dirpath, dirnames, filenames in os.walk(root_dir):
        dirnames[:] = [d for d in dirnames if d not in EXCLUDED_DIRS]
        for filename in filenames:
            _, ext = os.path.splitext(filename)
            if ext in TARGET_EXTENSIONS:
                full_path = os.path.join(dirpath, filename)
                search_hardcoded_values(full_path)

if __name__ == "__main__":
    current_dir = os.getcwd()
    print(f"🚀 Durchsuche Projekt unter: {current_dir}")
    scan_project_for_hardcoded_values(current_dir)

    with open("hardcoded_alarms.txt", "w", encoding="utf-8") as f:
        f.write('\n'.join(matches_summary))

    print("✅ Ergebnisse gespeichert in 'hardcoded_alarms.txt'")
