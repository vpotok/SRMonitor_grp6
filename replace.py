import os
import re

# Verzeichnisse, die durchsucht werden sollen
project_root = r'C:\Users\Alex\Downloads\Telegram Desktop\SRMonitor_grp6-gpg\SRMonitor_grp6-gpg'  # Dein Projektpfad
output_env_file = r'C:\Users\Alex\Downloads\Telegram Desktop\SRMonitor_grp6-gpg\SRMonitor_grp6-gpg\.env'  # Ausgabe der .env-Datei


# Regex-Pattern zum Extrahieren der relevanten Daten
patterns = {
    'URLs': r'(https?://[^\s]+)',
    'Ports': r'\b(\d{2,5})\b',
    'JWT Keywords': r'\b(jwt|JWT|Jwt)\b',
    'Token Keywords': r'\b(token|Authorization|Bearer)\b',
    'API Keys': r'\b(ApiKey)\b',
    'Passwords': r'\b(password|Token)\b',
    'IP Addresss': r'\b(\d{1,3}\.){3}\d{1,3}\b',
}

# Funktionen, um Dateien zu durchsuchen und Daten zu extrahieren
def extract_from_file(file_path, patterns):
    found_data = {}
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            content = file.read()
            for key, pattern in patterns.items():
                found_data[key] = set(re.findall(pattern, content))
    except Exception as e:
        print(f"Fehler beim Verarbeiten der Datei {file_path}: {e}")
    return found_data

def generate_env_file(project_root, output_env_file, patterns):
    # Sammle alle relevanten Daten aus den Dateien
    env_data = {}

    for root, dirs, files in os.walk(project_root):
        # Ignoriere bestimmte Verzeichnisse
        if '.git' in dirs:
            dirs.remove('.git')
        if 'node_modules' in dirs:
            dirs.remove('node_modules')
        if '.next' in dirs:
            dirs.remove('.next')
        if 'dist' in dirs:
            dirs.remove('dist')
        if '__pycache__' in dirs:
            dirs.remove('__pycache__')

        # Durchsuche alle .cs und .js/.tsx Dateien
        for file in files:
            if file.endswith(('.cs', '.js', '.tsx')):
                file_path = os.path.join(root, file)
                file_data = extract_from_file(file_path, patterns)

                # Füge die gefundenen Daten zur Umgebungsvariablen-Liste hinzu
                for key, values in file_data.items():
                    if values:
                        if key not in env_data:
                            env_data[key] = set()
                        env_data[key].update(values)

    # Schreibe die Daten in die .env-Datei
    with open(output_env_file, 'w', encoding='utf-8') as env_file:
        for key, values in env_data.items():
            if values:
                env_file.write(f'# {key}\n')
                for value in values:
                    env_file.write(f'{key.upper()}={value}\n')
                env_file.write('\n')

    print(f".env-Datei wurde erstellt: {output_env_file}")

# Skript ausführen
generate_env_file(project_root, output_env_file, patterns)
