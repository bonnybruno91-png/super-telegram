import React from 'react'
import Header from './components/Header'
import Hero from './components/Hero'
import HowItWorks from './components/HowItWorks'
import DualPath from './components/DualPath'
import MentorDirectory from './components/MentorDirectory'
import TrustSection from './components/TrustSection'
import Testimonials from './components/Testimonials'
import Footer from './components/Footer'

export default function App() {
  return (
    <div className="min-h-screen font-sans">
      <Header />
      <main>
        <Hero />
        <HowItWorks />
        <DualPath />
        <MentorDirectory />
        <TrustSection />
        <Testimonials />
      </main>
      <Footer />
    </div>
  )
}
