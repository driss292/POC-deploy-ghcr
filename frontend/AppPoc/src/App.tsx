import { useState } from "react";

function App() {
  const [content, setContent] = useState("");
  const apiUrl = import.meta.env.VITE_API_URL || "http://localhost:5144";

  const sendToBack = async () => {
    await fetch(`${apiUrl}/messages`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ content }),
    });
    alert("Message envoyé au Backend !");
    setContent("");
  };

  return (
    <div style={{ padding: "2rem" }}>
      <h1>POC Fullstack</h1>
      <input
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder="Écris coucou"
      />
      <button onClick={sendToBack}>Envoyer</button>
    </div>
  );
}

export default App;
