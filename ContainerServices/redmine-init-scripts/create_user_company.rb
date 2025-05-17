password_file = '/usr/src/redmine/init/company.txt'

if File.exist?(password_file)
  password = File.read(password_file).strip

  user = User.find_by(login: 'company')
  if user
    puts "ℹ️  Benutzer 'company' existiert bereits."
  else
    u = User.new(
      login: 'company',
      firstname: 'company',
      lastname: 'Dofirste',
      mail: 'company@example.com',
      password: password,
      password_confirmation: password
    )
    u.admin = false
    u.status = 1  # Aktiv
    u.language = 'de'
    u.must_change_passwd = false
    if u.save
      puts "✅ Benutzer 'company' wurde erstellt."
    else
      puts "❌ Fehler beim Erstellen von 'company': #{u.errors.full_messages.join(', ')}"
    end
  end
else
  puts "❌ Passwortdatei nicht gefunden: #{password_file}"
end
