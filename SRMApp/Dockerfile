FROM node:18-alpine
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy the rest of the app and build it
COPY . .
RUN npm run build

# Install 'serve' globally to serve the built app
RUN npm install -g serve

# Serve the files from the dist directory
WORKDIR /app/dist

EXPOSE 80
CMD ["serve", "-s", ".", "-l", "80"]
