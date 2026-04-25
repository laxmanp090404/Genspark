import fs from 'node:fs';
import path from 'node:path';
import dotenv from 'dotenv';

const projectRoot = process.cwd();
const envPath = path.join(projectRoot, '.env');
const outputPath = path.join(projectRoot, 'src/environments/environment.ts');

const env = fs.existsSync(envPath) ? dotenv.parse(fs.readFileSync(envPath)) : {};
const apiBaseUrl = env.NG_APP_API_BASE_URL ?? 'http://localhost:5017/api';

const fileContent = `export const environment = {
  production: false,
  apiBaseUrl: '${apiBaseUrl}'
};
`;

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, fileContent);
console.log(`Environment synced to ${outputPath}`);
