Setting.find_or_initialize_by(name: 'rest_api_enabled').tap do |s|
    s.value = '1'
    s.save!
  end
  
  puts "✅ REST API dauerhaft aktiviert"
  