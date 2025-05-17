password_file = '/usr/src/redmine/init/admin.txt'

if File.exist?(password_file)
  new_password = File.read(password_file).strip
  u = User.find_by(login: 'admin')
  if u
    u.password = new_password
    u.must_change_passwd = false
    u.save!
    puts "✅ Admin-Passwort wurde aktualisiert."
  else
    puts "❌ Kein Benutzer 'admin' gefunden."
  end
else
  puts "❌ Passwortdatei nicht gefunden: #{password_file}"
end
