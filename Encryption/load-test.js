import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = 'http://localhost:5207';

export const options = {
  stages: [
    { duration: '10s', target: 50 }, // Ramp-up na 10 súbežných používateľov
    { duration: '20s', target: 30 }, // Udržiavanie záťaže 20 používateľov
    { duration: '5s', target: 0 },   // Ramp-down na 0
  ],
  thresholds: {
    // 95% všetkých požiadaviek musí zbehnúť do 1.5s (berúc do úvahy náročný Argon2id)
    http_req_duration: ['p(99)<300'],
    // Chybovosť musí byť nižšia ako 5%
    http_req_failed: ['rate<0.01'],
  },
};

export function setup() {
  const loginPayload = JSON.stringify({
    email: 'tester1@test.com',
    password: 'Pass1234!',
  });

  const headers = { 'Content-Type': 'application/json' };
  const res = http.post(`${BASE_URL}/api/auth/login`, loginPayload, { headers });

  check(res, {
    'login status is 200': (r) => r.status === 200,
  });

  const token = res.json('token');
  return { token };
}

export default function (data) {
  const authHeaders = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${data.token}`,
  };

  // 1. Krok: Vytvorenie šifrovaného odkazu
  const createPayload = JSON.stringify({
    originalUrl: `https://example.com/test-${Date.now()}`,
    passphrase: 'LoadTestPassphrase123!',
    expiresInMinutes: 60,
    maxAccessCount: 50,
    burnOnRead: false,
  });

  const createRes = http.post(`${BASE_URL}/api/links/create`, createPayload, {
    headers: authHeaders,
  });

  check(createRes, {
    'create status is 201': (r) => r.status === 201,
  });

  const shortCode = createRes.json('shortCode');

  if (shortCode) {
    // 2. Krok: Dešifrovanie odkazu (stresuje Argon2id a Nginx balancer)
    const decryptPayload = JSON.stringify({
      passphrase: 'LoadTestPassphrase123!',
    });

    const decryptHeaders = { 'Content-Type': 'application/json' };
    const decryptRes = http.post(
      `${BASE_URL}/api/links/${shortCode}/decrypt`,
      decryptPayload,
      { headers: decryptHeaders }
    );

    check(decryptRes, {
      'decrypt status is 200': (r) => r.status === 200,
    });
  }

  sleep(0.5);
}