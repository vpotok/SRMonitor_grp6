// mock/device.js
export default [
    {
      url: '/api/devices',
      method: 'get',
      response: () => {
        return [
          {
            id: 'device-001',
            installed_date: '2023-01-15',
            battery_state: 'full',
            version: 'v1.0.3',
            owner: 'user123'
          },
          {
            id: 'device-002',
            installed_date: '2022-12-05',
            battery_state: 'low',
            version: 'v1.0.1',
            owner: 'user456'
          }
        ]
      }
    }
  ]
  