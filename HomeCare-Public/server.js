const express = require('express');
const path = require('path');
const app = express();

app.use(express.static(path.join(__dirname, 'dist/homecare-public/browser')));

app.get('/{*path}', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist/homecare-public/browser/index.html'));
});

const port = process.env.PORT || 8082;
app.listen(port, () => console.log(`Public running on port ${port}`));