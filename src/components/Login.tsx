// replace AdminInitiateAuth usage with a backend call
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();
  const resp = await fetch('/api/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  });
  if (!resp.ok) {
    const body = await resp.json();
    setError(body.message || 'Login failed');
    return;
  }
  const data = await resp.json();
  // data should include tokens returned by backend
  console.log('Login success', data);
  navigate('/productlist');
};