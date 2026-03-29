import React from 'react'
import { motion } from 'framer-motion'
import { ArrowRight, Users, Star, ShieldCheck, PlayCircle } from 'lucide-react'

const fadeUp = {
  hidden: { opacity: 0, y: 30 },
  show: (i = 0) => ({
    opacity: 1,
    y: 0,
    transition: { duration: 0.6, delay: i * 0.12, ease: 'easeOut' },
  }),
}

const stats = [
  { icon: Users, value: '2,400+', label: 'Active Mentors' },
  { icon: Star, value: '4.9 / 5', label: 'Average Rating' },
  { icon: ShieldCheck, value: '100%', label: 'License Verified' },
]

export default function Hero() {
  return (
    <section className="relative min-h-screen flex items-center overflow-hidden bg-white pt-20">
      {/* Background decoration */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 right-0 w-[600px] h-[600px] rounded-full bg-gradient-to-bl from-teal-50 via-teal-100/40 to-transparent -translate-y-1/4 translate-x-1/4" />
        <div className="absolute bottom-0 left-0 w-[400px] h-[400px] rounded-full bg-gradient-to-tr from-ocean-50/60 to-transparent translate-y-1/4 -translate-x-1/4" />
        {/* Subtle grid */}
        <div
          className="absolute inset-0 opacity-[0.025]"
          style={{
            backgroundImage: `url("data:image/svg+xml,%3Csvg width='40' height='40' viewBox='0 0 40 40' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Ccircle fill='%230d9488' cx='20' cy='20' r='1'/%3E%3C/g%3E%3C/svg%3E")`,
          }}
        />
      </div>

      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 md:py-24">
        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
          {/* Left — Text */}
          <div className="text-center lg:text-left">
            {/* Badge */}
            <motion.div
              variants={fadeUp}
              initial="hidden"
              animate="show"
              custom={0}
              className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-teal-50 border border-teal-100 text-teal-700 text-sm font-medium mb-6"
            >
              <span className="w-2 h-2 rounded-full bg-teal-500 animate-pulse" />
              Trusted by Physio Professionals Worldwide
            </motion.div>

            {/* Headline */}
            <motion.h1
              variants={fadeUp}
              initial="hidden"
              animate="show"
              custom={1}
              className="text-4xl sm:text-5xl md:text-6xl font-extrabold leading-tight tracking-tight text-gray-900 mb-6"
            >
              Find Your Path with a{' '}
              <span className="relative inline-block">
                <span className="relative z-10 text-transparent bg-clip-text bg-gradient-to-r from-teal-600 to-teal-700">
                  Trusted Mentor
                </span>
                <svg
                  className="absolute -bottom-2 left-0 w-full"
                  viewBox="0 0 300 12"
                  fill="none"
                  preserveAspectRatio="none"
                >
                  <path
                    d="M2 8 C60 3, 120 10, 180 6 S260 2, 298 7"
                    stroke="#0d9488"
                    strokeWidth="3"
                    strokeLinecap="round"
                    fill="none"
                    opacity="0.4"
                  />
                </svg>
              </span>{' '}
              in Physiotherapy
            </motion.h1>

            {/* Subtext */}
            <motion.p
              variants={fadeUp}
              initial="hidden"
              animate="show"
              custom={2}
              className="text-lg md:text-xl text-gray-500 leading-relaxed mb-10 max-w-xl mx-auto lg:mx-0"
            >
              PhysioMentor bridges the gap between seasoned physiotherapists and the next
              generation of practitioners. One-on-one guidance, real clinical insight, lifelong
              impact.
            </motion.p>

            {/* CTA Buttons */}
            <motion.div
              variants={fadeUp}
              initial="hidden"
              animate="show"
              custom={3}
              className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start"
            >
              <a
                href="#dual-path"
                className="group flex items-center justify-center gap-2 px-7 py-4 text-base font-semibold text-white bg-emerald-500 hover:bg-emerald-600 rounded-2xl shadow-lg shadow-emerald-200 hover:shadow-emerald-300 hover:-translate-y-0.5 transition-all duration-200"
              >
                Become a Mentor
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </a>
              <a
                href="#mentors"
                className="group flex items-center justify-center gap-2 px-7 py-4 text-base font-semibold text-white bg-ocean-500 hover:bg-ocean-600 rounded-2xl shadow-lg shadow-sky-200 hover:shadow-sky-300 hover:-translate-y-0.5 transition-all duration-200"
              >
                Find a Mentor
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </a>
            </motion.div>

            {/* Social proof */}
            <motion.div
              variants={fadeUp}
              initial="hidden"
              animate="show"
              custom={4}
              className="mt-10 flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-6"
            >
              {/* Avatars */}
              <div className="flex items-center gap-3">
                <div className="flex -space-x-2.5">
                  {['bg-teal-400', 'bg-emerald-400', 'bg-sky-400', 'bg-violet-400'].map((c, i) => (
                    <div
                      key={i}
                      className={`w-9 h-9 rounded-full ${c} border-2 border-white flex items-center justify-center text-white text-xs font-bold shadow-sm`}
                    >
                      {['JK', 'AL', 'MS', 'DR'][i]}
                    </div>
                  ))}
                </div>
                <p className="text-sm text-gray-500">
                  <span className="font-semibold text-gray-800">1,200+ students</span> already learning
                </p>
              </div>
              <div className="hidden sm:block w-px h-8 bg-gray-200" />
              <div className="flex items-center gap-1.5">
                {[1, 2, 3, 4, 5].map((s) => (
                  <Star key={s} className="w-4 h-4 fill-amber-400 text-amber-400" />
                ))}
                <span className="text-sm text-gray-500 ml-1">Rated 4.9 by mentees</span>
              </div>
            </motion.div>
          </div>

          {/* Right — Visual Card */}
          <motion.div
            initial={{ opacity: 0, x: 40 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.8, delay: 0.3, ease: 'easeOut' }}
            className="relative hidden lg:block"
          >
            {/* Main card */}
            <div className="relative bg-gradient-to-br from-teal-600 to-teal-800 rounded-3xl p-8 shadow-2xl shadow-teal-900/20 overflow-hidden">
              {/* Decorative circles */}
              <div className="absolute top-0 right-0 w-48 h-48 rounded-full bg-white/5 -translate-y-12 translate-x-12" />
              <div className="absolute bottom-0 left-0 w-32 h-32 rounded-full bg-white/5 translate-y-8 -translate-x-8" />

              {/* Video / placeholder */}
              <div className="relative bg-white/10 backdrop-blur rounded-2xl p-6 mb-6 flex items-center gap-4">
                <div className="flex-shrink-0 w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center">
                  <PlayCircle className="w-8 h-8 text-white" />
                </div>
                <div>
                  <p className="text-white font-semibold text-sm">Watch how it works</p>
                  <p className="text-teal-200 text-xs mt-0.5">2 min overview</p>
                </div>
              </div>

              {/* Stats */}
              <div className="grid grid-cols-3 gap-4">
                {stats.map(({ icon: Icon, value, label }) => (
                  <div key={label} className="bg-white/10 backdrop-blur rounded-xl p-4 text-center">
                    <Icon className="w-5 h-5 text-teal-200 mx-auto mb-2" />
                    <p className="text-white font-bold text-lg leading-none">{value}</p>
                    <p className="text-teal-200 text-xs mt-1 leading-snug">{label}</p>
                  </div>
                ))}
              </div>

              {/* Mentor preview */}
              <div className="mt-6 bg-white/10 rounded-2xl p-5 flex items-center gap-4">
                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-emerald-400 to-teal-500 flex items-center justify-center text-white font-bold text-sm flex-shrink-0">
                  DR
                </div>
                <div className="flex-1">
                  <p className="text-white font-semibold text-sm">Dr. Rachel Moreau</p>
                  <p className="text-teal-200 text-xs">Sports Physiotherapist · Paris</p>
                </div>
                <div className="flex items-center gap-1 bg-emerald-400/20 px-2.5 py-1 rounded-full">
                  <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                  <span className="text-emerald-300 text-xs font-medium">Available</span>
                </div>
              </div>
            </div>

            {/* Floating badge */}
            <motion.div
              animate={{ y: [0, -8, 0] }}
              transition={{ duration: 4, repeat: Infinity, ease: 'easeInOut' }}
              className="absolute -left-6 top-16 bg-white rounded-2xl shadow-xl shadow-gray-200/60 p-4 flex items-center gap-3 border border-gray-100"
            >
              <div className="w-10 h-10 rounded-xl bg-teal-50 flex items-center justify-center">
                <ShieldCheck className="w-5 h-5 text-teal-600" />
              </div>
              <div>
                <p className="text-xs font-semibold text-gray-800">License Verified</p>
                <p className="text-xs text-gray-400">RPPS Certified</p>
              </div>
            </motion.div>

            <motion.div
              animate={{ y: [0, 8, 0] }}
              transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut', delay: 1 }}
              className="absolute -right-6 bottom-20 bg-white rounded-2xl shadow-xl shadow-gray-200/60 p-4 border border-gray-100"
            >
              <div className="flex items-center gap-2 mb-1">
                {[1, 2, 3, 4, 5].map((s) => (
                  <Star key={s} className="w-3 h-3 fill-amber-400 text-amber-400" />
                ))}
              </div>
              <p className="text-xs font-semibold text-gray-800">"Life-changing guidance"</p>
              <p className="text-xs text-gray-400 mt-0.5">— Sofia T., Mentee</p>
            </motion.div>
          </motion.div>
        </div>
      </div>
    </section>
  )
}
