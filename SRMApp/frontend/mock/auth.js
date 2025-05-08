// mock/auth.js
import jwt from 'jsonwebtoken'

const SECRET = 'mock_secret' // Keep consistent for decoding

export default [
  {
    url: '/api/login',
    method: 'post',
    response: ({ body }) => {
      const { username, password } = body

      if (username === 'admin' && password === 'admin123') {
        const token = jwt.sign({ sub: '1', role: 'admin' }, SECRET, { expiresIn: '1h' })
        return { token }
      }

      if (username === 'customer' && password === 'cust123') {
        const token = jwt.sign({ sub: '2', role: 'customer' }, SECRET, { expiresIn: '1h' })
        return { token }
      }

      return {
        code: 401,
        message: 'Invalid credentials'
      }
    }
  }
]
