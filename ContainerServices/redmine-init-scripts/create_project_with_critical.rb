# Projekt
project = Project.find_or_initialize_by(identifier: 'issue-tracker')
project.name = 'Issue Tracker'
project.description = 'Projekt für kritische Tickets'
project.is_public = false
project.save!

# Tracker
tracker = Tracker.find_or_initialize_by(name: 'Event')
tracker.default_status ||= IssueStatus.find_or_create_by!(name: 'Neu', is_closed: false)
tracker.save!
project.trackers << tracker unless project.trackers.include?(tracker)

# Status
IssueStatus.find_or_create_by!(name: 'Neu') do |s|
  s.is_closed = false
  s.is_default = true
end

# Priorität
IssuePriority.find_or_create_by!(name: 'Critical') do |p|
  p.is_default = false
  p.active = true
end

project = Project.find_by(identifier: 'issue-tracker')
user = User.find_by(login: 'company')

# Stelle sicher, dass eine Rolle existiert
role = Role.find_or_create_by!(name: 'Manager') do |r|
    r.assignable = true
    r.permissions = [
      :view_project,
      :view_issues,
      :add_issues
    ]
  end
  

if project.nil?
  puts "❌ Projekt 'issue-tracker' nicht gefunden."
elsif user.nil?
  puts "❌ Benutzer 'company' nicht gefunden."
elsif role.nil?
  puts "❌ Keine gültige Rolle gefunden."
else
  member = Member.find_or_initialize_by(project: project, user: user)
  member.role_ids = [role.id]  # wichtige Zuweisung!
  if member.save
    puts "✅ Benutzer 'company' wurde dem Projekt '#{project.name}' als '#{role.name}' zugewiesen."
  else
    puts "❌ Fehler beim Speichern der Mitgliedschaft: #{member.errors.full_messages.join(", ")}"
  end
end
