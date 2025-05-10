Username	Password	Role
admin	admin123	admin
customer	cust123	customer


curl -X POST http://localhost:5266/api/auth/login \
-H "Content-Type: application/json" \
-d '{
  "username": "admin",
  "password": "admin123"
}'



winx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations list --project SRMCore
winx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations add CreateUsersTable --project SRMCore
dwinx@DESKTOP-PMVN4J3:~/SRMonitor_grp6$ dotnet ef migrations add AddDevicesTable --project SRMCore



docker exec -it <container_id> sh

dotnet build
dotnet ef migrations add InitialCreate --project SRMCore
dotnet ef database update --project SRMCore


docker exec -it postgres psql -U postgres -d srmcore
SELECT * FROM "Users";
SELECT * FROM "Devices";


docker-compose up --build
docker exec -it postgres psql -U postgres -d srmcore







curl -X POST http://localhost:5277/api/auth/generate-token \
-H "Content-Type: application/json" \
-d '{"username": "admin", "role": "admin"}'


sudo docker exec -it redis redis-cli


127.0.0.1:6379> KEYS *
1) "token:admin"
127.0.0.1:6379> GET token:admin



docker exec -it srmonitor_grp6-srmcore-1 sh
curl -X POST http://srmauth:80/api/auth/generate-token \
-H "Content-Type: application/json" \
-d '{"username": "admin", "role": "admin"}'



### **Token Expiration Handling**

#### **Overview**
This implementation introduces a **Token Rotation** mechanism to securely handle token expiration in the authentication system. It uses a two-token approach with short-lived `auth_token`s (JWTs) for authentication and long-lived `refresh_token`s for session management. The `refresh_token` allows seamless renewal of expired `auth_token`s without requiring the user to log in again.

---

#### **Key Features**
1. **Short-Lived Access Tokens (`auth_token`)**:
   - JWTs with a lifespan of 15 minutes.
   - Used for authenticating API requests.
   - Stored in `HttpOnly` cookies to prevent XSS attacks.

2. **Long-Lived Refresh Tokens (`refresh_token`)**:
   - Valid for 7 days and stored securely in Redis.
   - Used to generate new `auth_token`s when the current one expires.
   - Rotated with each use to mitigate replay attacks.

3. **Token Rotation**:
   - When a `refresh_token` is used, a new `auth_token` and `refresh_token` are issued.
   - The old `refresh_token` is invalidated to prevent reuse.

4. **Automatic Token Refresh**:
   - The backend middleware (`JwtMiddleware`) automatically refreshes expired `auth_token`s using the `refresh_token`.
   - The frontend retries failed requests (`401 Unauthorized`) by refreshing the token and resending the request.

5. **Centralized Token Management**:
   - All token generation, validation, and storage are handled by the SRMAuth service.
   - The SRMCore service acts as a proxy, forwarding requests to SRMAuth.

---

#### **How It Works**
1. **Login**:
   - The user logs in and receives both an `auth_token` and a `refresh_token`.
   - The `auth_token` is valid for 15 minutes, while the `refresh_token` is valid for 7 days.

2. **Accessing Protected Resources**:
   - The `auth_token` is sent with each request to authenticate the user.
   - If the `auth_token` is valid, the request is processed.

3. **Token Expiration**:
   - If the `auth_token` expires, the `JwtMiddleware` or frontend automatically uses the `refresh_token` to obtain a new `auth_token`.

4. **Token Rotation**:
   - When a `refresh_token` is used, a new `auth_token` and `refresh_token` are issued.
   - The old `refresh_token` is invalidated in Redis.

5. **Logout**:
   - Both `auth_token` and `refresh_token` cookies are cleared, and the session is terminated.

---

#### **Advantages**
- **Enhanced Security**:
  - Short-lived `auth_token`s minimize the impact of token theft.
  - Rotating `refresh_token`s prevent replay attacks.
  - Tokens are stored in `HttpOnly` cookies, reducing the risk of XSS attacks.

- **Seamless User Experience**:
  - Users remain logged in as long as their `refresh_token` is valid.
  - Automatic token refresh ensures uninterrupted access to protected resources.

- **Scalability**:
  - Stateless `auth_token`s reduce server load.
  - Redis provides a scalable solution for managing `refresh_token`s in distributed systems.

---

#### **Key Components**
1. **SRMAuth Service**:
   - Handles token generation, validation, and refresh token storage.
   - Stores `refresh_token`s in Redis with a 7-day TTL.

2. **SRMCore Service**:
   - Provides endpoints for login, token refresh, and role retrieval.
   - Uses middleware to validate and refresh tokens automatically.

3. **JwtMiddleware**:
   - Validates `auth_token`s for each request.
   - Refreshes expired `auth_token`s using the `refresh_token`.

4. **Frontend**:
   - Handles `401 Unauthorized` responses by refreshing tokens and retrying requests.

---

#### **Testing**
1. **Login**:
   - Verify that both `auth_token` and `refresh_token` cookies are set after login.

2. **Access Protected Resources**:
   - Test accessing protected endpoints with a valid `auth_token`.

3. **Token Expiration**:
   - Wait for the `auth_token` to expire and verify that the token is refreshed automatically.

4. **Invalid Refresh Token**:
   - Test with an invalid or missing `refresh_token` and verify that the request fails with `401 Unauthorized`.

5. **Logout**:
   - Verify that both `auth_token` and `refresh_token` cookies are cleared after logout.

---

#### **Conclusion**
This implementation provides a secure and scalable solution for handling token expiration. By combining short-lived `auth_token`s with long-lived `refresh_token`s and implementing token rotation, it ensures both security and a seamless user experience. The centralized token management in SRMAuth and the automatic refresh mechanism in SRMCore make this architecture robust and maintainable.
