import React from 'react'
import { motion } from 'framer-motion'
import { useInView } from 'framer-motion'
import { useRef } from 'react'
import { UserPlus, Search, CalendarCheck, TrendingUp } from 'lucide-react'

const steps = [
  {
    icon: UserPlus,
    step: '01',
    title: 'Create Your Profile',
    description:
      'Sign up in minutes. Mentors upload credentials and areas of expertise. Students share their current year, university, and learning goals.',
    color: 'teal',
    bg: 'bg-teal-50',
    iconBg: 'bg-teal-600',
    border: 'border-teal-100',
  },
  {
    icon: Search,
    step: '02',
    title: 'Find the Perfect Match',
    description:
      'Browse verified mentors by specialty, availability, language, and location. Our smart algorithm surfaces the best matches for your specific needs.',
    color: 'ocean',
    bg: 'bg-sky-50',
    iconBg: 'bg-sky-500',
    border: 'border-sky-100',
  },
  {
    icon: CalendarCheck,
    step: '03',
    title: 'Book a Session',
    description:
      'Request a first free 15-minute discovery call. Then schedule regular mentorship sessions at a pace that works for both of you.',
    color: 'emerald',
    bg: 'bg-emerald-50',
    iconBg: 'bg-emerald-500',
    border: 'border-emerald-100',
  },
  {
    icon: TrendingUp,
    step: '04',
    title: 'Grow Together',
    description:
      'Track progress, share resources, and build a lasting professional relationship. Many PhysioMentor connections result in long-term career partnerships.',
    color: 'violet',
    bg: 'bg-violet-50',
    iconBg: 'bg-violet-500',
    border: 'border-violet-100',
  },
]

function StepCard({ step, index }) {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-80px' })

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 40 }}
      animate={inView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.55, delay: index * 0.12, ease: 'easeOut' }}
      className={`relative group rounded-2xl border ${step.border} ${step.bg} p-6 md:p-8 hover:shadow-lg hover:-translate-y-1 transition-all duration-300 cursor-default`}
    >
      <div className="flex items-start gap-5">
        <div className={`flex-shrink-0 w-12 h-12 rounded-xl ${step.iconBg} flex items-center justify-center shadow-sm`}>
          <step.icon className="w-6 h-6 text-white" />
        </div>
        <div className="flex-1">
          <span className="text-xs font-bold text-gray-400 tracking-widest uppercase mb-1 block">
            Step {step.step}
          </span>
          <h3 className="text-lg font-bold text-gray-900 mb-2">{step.title}</h3>
          <p className="text-sm text-gray-600 leading-relaxed">{step.description}</p>
        </div>
      </div>
      {/* Connector dot */}
      <div className="absolute -right-3 top-1/2 -translate-y-1/2 w-6 h-6 rounded-full bg-white border-2 border-gray-200 hidden lg:flex items-center justify-center last:hidden">
        <div className="w-2 h-2 rounded-full bg-gray-300" />
      </div>
    </motion.div>
  )
}

export default function HowItWorks() {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })

  return (
    <section id="how-it-works" className="py-20 md:py-28 bg-white">
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
            How It Works
          </span>
          <h2 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-5">
            Your journey from student to expert{' '}
            <span className="text-teal-600">starts here</span>
          </h2>
          <p className="text-lg text-gray-500 max-w-2xl mx-auto leading-relaxed">
            Getting started with PhysioMentor is simple. Whether you're looking to learn or share
            your expertise, we've designed every step to be seamless.
          </p>
        </motion.div>

        {/* Steps Grid */}
        <div className="grid sm:grid-cols-2 gap-5 md:gap-6">
          {steps.map((step, i) => (
            <StepCard key={step.step} step={step} index={i} />
          ))}
        </div>

        {/* Bottom CTA */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6, delay: 0.6 }}
          className="text-center mt-14"
        >
          <a
            href="#dual-path"
            className="inline-flex items-center gap-2 px-8 py-4 bg-teal-600 hover:bg-teal-700 text-white font-semibold rounded-2xl shadow-lg shadow-teal-200 hover:shadow-teal-300 hover:-translate-y-0.5 transition-all duration-200 text-base"
          >
            Start Your Journey Today
          </a>
        </motion.div>
      </div>
    </section>
  )
}
