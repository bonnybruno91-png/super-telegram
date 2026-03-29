import React from 'react'
import { motion } from 'framer-motion'
import { useInView } from 'framer-motion'
import { useRef } from 'react'
import {
  Stethoscope,
  GraduationCap,
  CheckCircle2,
  ArrowRight,
  BadgeCheck,
  BookOpen,
  Users,
  Award,
} from 'lucide-react'

const mentorPerks = [
  { icon: Users, text: 'Connect with motivated students' },
  { icon: Award, text: 'Build your professional reputation' },
  { icon: BadgeCheck, text: 'Flexible scheduling on your terms' },
  { icon: CheckCircle2, text: 'Earn while giving back to the field' },
]

const menteePerks = [
  { icon: BookOpen, text: 'Learn from verified clinicians' },
  { icon: Stethoscope, text: 'Specialty-matched guidance' },
  { icon: BadgeCheck, text: 'Affordable 1-on-1 sessions' },
  { icon: CheckCircle2, text: 'Accelerate your clinical growth' },
]

function PathCard({ side, delay }) {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-80px' })

  const isMentor = side === 'mentor'
  const perks = isMentor ? mentorPerks : menteePerks

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 40 }}
      animate={inView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.65, delay, ease: 'easeOut' }}
      className={`group relative rounded-3xl overflow-hidden border-2 transition-all duration-300 hover:-translate-y-2 hover:shadow-2xl cursor-pointer ${
        isMentor
          ? 'border-teal-100 hover:border-teal-300 hover:shadow-teal-100/60'
          : 'border-sky-100 hover:border-sky-300 hover:shadow-sky-100/60'
      } bg-white`}
    >
      {/* Top gradient band */}
      <div
        className={`h-2 w-full ${
          isMentor
            ? 'bg-gradient-to-r from-teal-500 to-emerald-500'
            : 'bg-gradient-to-r from-sky-500 to-blue-600'
        }`}
      />

      <div className="p-8 md:p-10">
        {/* Icon + Badge */}
        <div className="flex items-start justify-between mb-6">
          <div
            className={`w-16 h-16 rounded-2xl flex items-center justify-center shadow-md ${
              isMentor ? 'bg-teal-600' : 'bg-sky-500'
            }`}
          >
            {isMentor ? (
              <Stethoscope className="w-8 h-8 text-white" />
            ) : (
              <GraduationCap className="w-8 h-8 text-white" />
            )}
          </div>
          <span
            className={`text-xs font-bold px-3 py-1.5 rounded-full border ${
              isMentor
                ? 'text-teal-700 bg-teal-50 border-teal-100'
                : 'text-sky-700 bg-sky-50 border-sky-100'
            }`}
          >
            {isMentor ? 'For Professionals' : 'For Students'}
          </span>
        </div>

        {/* Title */}
        <h3 className="text-2xl md:text-3xl font-extrabold text-gray-900 mb-2">
          {isMentor ? 'Professional Physio' : 'Physio Student'}
        </h3>
        <p className="text-gray-500 text-sm leading-relaxed mb-8">
          {isMentor
            ? 'Share decades of clinical expertise, shape the next generation of practitioners, and join a growing community of physio leaders.'
            : 'Connect with experienced mentors who can fast-track your clinical skills, prepare you for exams, and help you navigate your career.'}
        </p>

        {/* Perks list */}
        <ul className="space-y-3 mb-10">
          {perks.map(({ icon: Icon, text }) => (
            <li key={text} className="flex items-center gap-3 text-sm text-gray-700">
              <Icon
                className={`w-4 h-4 flex-shrink-0 ${
                  isMentor ? 'text-teal-600' : 'text-sky-500'
                }`}
              />
              {text}
            </li>
          ))}
        </ul>

        {/* CTA */}
        <a
          href="#"
          className={`group/btn w-full flex items-center justify-center gap-2 py-4 rounded-2xl font-semibold text-base transition-all duration-200 hover:-translate-y-0.5 shadow-lg ${
            isMentor
              ? 'bg-teal-600 hover:bg-teal-700 text-white shadow-teal-200 hover:shadow-teal-300'
              : 'bg-sky-500 hover:bg-sky-600 text-white shadow-sky-200 hover:shadow-sky-300'
          }`}
        >
          {isMentor ? 'Register as a Mentor' : 'Find My Mentor'}
          <ArrowRight className="w-5 h-5 group-hover/btn:translate-x-1 transition-transform" />
        </a>
      </div>
    </motion.div>
  )
}

export default function DualPath() {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })

  return (
    <section id="dual-path" className="py-20 md:py-28 bg-sage-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 24 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
          className="text-center mb-16"
        >
          <span className="inline-block text-xs font-bold text-teal-600 tracking-widest uppercase mb-4 px-3 py-1.5 bg-teal-50 rounded-full border border-teal-100">
            Choose Your Path
          </span>
          <h2 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-5">
            Are you a...
          </h2>
          <p className="text-lg text-gray-500 max-w-xl mx-auto">
            PhysioMentor serves two communities. Select the path that describes you best to get
            started.
          </p>
        </motion.div>

        {/* Dual Cards */}
        <div className="grid md:grid-cols-2 gap-6 lg:gap-8 max-w-4xl mx-auto">
          <PathCard side="mentor" delay={0.1} />
          <PathCard side="mentee" delay={0.22} />
        </div>

        {/* Divider note */}
        <motion.p
          initial={{ opacity: 0 }}
          animate={inView ? { opacity: 1 } : {}}
          transition={{ delay: 0.6 }}
          className="text-center text-sm text-gray-400 mt-10"
        >
          Already have an account?{' '}
          <a href="#" className="text-teal-600 hover:underline font-medium">
            Sign in here
          </a>
        </motion.p>
      </div>
    </section>
  )
}
