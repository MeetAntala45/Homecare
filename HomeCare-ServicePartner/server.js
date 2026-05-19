const express = require('express');
const path = require('path');
const app = express();

app.use(express.static(path.join(__dirname, 'dist/HomeCare-ServicePartner/browser')));

app.get('/{*path}', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist/HomeCare-ServicePartner/browser/index.html'));
});

const port = process.env.PORT || 8083;
app.listen(port, () => console.log(`Partner running on port ${port}`));