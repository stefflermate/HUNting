import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import Header from "./components/layout/Header";

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
    <div className="bg-gradient-to-b bg-yellow-50 min-h-screen pt-16 px-4">
    <Header />
    <main className="max-w-4xl mx-auto py-10 text-center">
      <h1 className="text-4xl font-bold text-yellow-900 mb-6">
        Üdvözlünk a VadászVilágban!
      </h1>
      <p className="text-lg text-yellow-800 max-w-prose mx-auto">
        Fedezd fel a legszebb vadászterületeket, eseményeket és csatlakozz a természet szerelmeseinek közösségéhez!
      </p>
    </main>
  </div>
    </>
  )
}

export default App
