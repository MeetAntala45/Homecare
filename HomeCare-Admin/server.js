const express = require('express');
const path = require('path');
const app = express();

app.use(express.static(path.join(__dirname, 'dist/homecare-public/browser')));

app.get('/{*path}', (req, res) => {
    res.sendFile(path.join(__dirname, 'dist/homecare-public/browser/index.html'));
});

const port = process.env.PORT || 8080;
app.listen(port, () => console.log(`Admin running on port ${port}`));