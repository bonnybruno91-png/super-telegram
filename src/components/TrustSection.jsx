import React, { useRef } from 'react'
import { motion } from 'framer-motion'
import { useInView } from 'framer-motion'
import {
  ShieldCheck,
  FileCheck,
  Lock,
  Eye,
  BadgeCheck,
  Star,
  Users,
  MessageSquare,
} from 'lucide-react'

const verificationSteps = [
  {
    step: '1',
    icon: FileCheck,
    title: 'License Submission',
    description:
      'Every mentor submits their official physiotherapy license number (e.g., RPPS in France, HCPC in the UK). We accept credentials from 40+ regulatory bodies worldwide.',
    color: 'teal',
  },
  {
    step: '2',
    icon: Eye,
    title: 'Manual Review',
    description:
      'Our clinical compliance team cross-checks each submission against official national registries. No automated rubber-stamping — every profile is reviewed by a human.',
    color: 'sky',
  },
  {
    step: '3',
    icon: BadgeCheck,
    title: 'Verified Badge Awarded',
    description:
      'Upon passing, the mentor receives a "Verified" badge on their profile. Licenses are re-checked annually and after any reported concern.',
    color: 'emerald',
  },
  {
    step: '4',
    icon: Lock,
    title: 'Ongoing Monitoring',
    description:
      "PhysioMentor subscribes to real-time registry alerts. If a license is suspended or revoked, the mentor's account is immediately flagged and suspended.",
    color: 'violet',
  },
]

const trustPoints = [
  {
    icon: ShieldCheck,
    title: 'GDPR Compliant',
    desc: 'All personal and clinical data is stored with full EU GDPR compliance. You own your data — always.',
    bg: 'bg-teal-50',
    iconColor: 'text-teal-600',
  },
  {
    icon: Lock,
    title: 'End-to-End Encrypted Messages',
    desc: 'Session communications are encrypted in transit and at rest. Your conversations remain private.',
    bg: 'bg-sky-50',
    iconColor: 'text-sky-600',
  },
  {
    icon: Star,
    title: 'Community Ratings',
    desc: 'Every session is rated. Mentors with consistently low scores are reviewed and removed from the platform.',
    bg: 'bg-amber-50',
    iconColor: 'text-amber-600',
  },
  {
    icon: Users,
    title: 'Safe Reporting',
    desc: 'Anonymous reporting channels allow mentees to flag concerns. All reports are investigated within 24 hours.',
    bg: 'bg-rose-50',
    iconColor: 'text-rose-600',
  },
  {
    icon: MessageSquare,
    title: 'Session Summaries',
    desc: 'Structured post-session summaries keep mentorship transparent, accountable, and focused on goals.',
    bg: 'bg-violet-50',
    iconColor: 'text-violet-600',
  },
  {
    icon: FileCheck,
    title: 'Annual Re-Verification',
    desc: 'Mentor licenses are renewed annually to ensure all credentials remain current and valid.',
    bg: 'bg-emerald-50',
    iconColor: 'text-emerald-600',
  },
]

function VerifyStep({ step, index }) {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-60px' })

  const colors = {
    teal: 'bg-teal-600 border-teal-200 text-teal-600',
    sky: 'bg-sky-500 border-sky-200 text-sky-600',
    emerald: 'bg-emerald-500 border-emerald-200 text-emerald-600',
    violet: 'bg-violet-500 border-violet-200 text-violet-600',
  }

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, x: -24 }}
      animate={inView ? { opacity: 1, x: 0 } : {}}
      transition={{ duration: 0.55, delay: index * 0.12 }}
      className="flex gap-5"
    >
      {/* Step indicator + connector */}
      <div className="flex flex-col items-center flex-shrink-0">
        <div className={`w-10 h-10 rounded-xl bg-gradient-to-br ${
          step.color === 'teal' ? 'from-teal-500 to-teal-700' :
          step.color === 'sky' ? 'from-sky-400 to-sky-600' :
          step.color === 'emerald' ? 'from-emerald-400 to-emerald-600' :
          'from-violet-400 to-violet-600'
        } flex items-center justify-center text-white font-bold text-sm shadow-sm flex-shrink-0`}>
          {step.step}
        </div>
        {index < verificationSteps.length - 1 && (
          <div className="w-0.5 flex-1 bg-gray-200 mt-2 min-h-8" />
        )}
      </div>

      {/* Content */}
      <div className="pb-8">
        <div className="flex items-center gap-2 mb-2">
          <step.icon className={`w-4 h-4 ${colors[step.color].split(' ')[2]}`} />
          <h4 className="font-bold text-gray-900 text-base">{step.title}</h4>
        </div>
        <p className="text-sm text-gray-500 leading-relaxed">{step.description}</p>
      </div>
    </motion.div>
  )
}

export default function TrustSection() {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })

  return (
    <section id="trust" className="py-20 md:py-28 bg-sage-50">
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
            Trust & Safety
          </span>
          <h2 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-5">
            Your safety is our{' '}
            <span className="text-teal-600">first priority</span>
          </h2>
          <p className="text-lg text-gray-500 max-w-2xl mx-auto">
            We believe that genuine mentorship requires genuine trust. That's why every mentor
            on PhysioMentor is thoroughly verified before they can connect with a single student.
          </p>
        </motion.div>

        <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-start">
          {/* Left — Verification Process */}
          <div>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={inView ? { opacity: 1, y: 0 } : {}}
              transition={{ delay: 0.2 }}
              className="bg-white rounded-3xl border border-gray-100 shadow-sm p-8"
            >
              <div className="flex items-center gap-3 mb-8">
                <div className="w-10 h-10 rounded-xl bg-teal-600 flex items-center justify-center">
                  <ShieldCheck className="w-5 h-5 text-white" />
                </div>
                <div>
                  <h3 className="font-bold text-gray-900">Mentor Verification Process</h3>
                  <p className="text-xs text-gray-500">How we validate every credential</p>
                </div>
              </div>

              {verificationSteps.map((step, i) => (
                <VerifyStep key={step.step} step={step} index={i} />
              ))}
            </motion.div>
          </div>

          {/* Right — Trust Points Grid */}
          <div>
            <div className="grid sm:grid-cols-2 gap-4">
              {trustPoints.map((point, i) => (
                <motion.div
                  key={point.title}
                  initial={{ opacity: 0, y: 24 }}
                  animate={inView ? { opacity: 1, y: 0 } : {}}
                  transition={{ duration: 0.5, delay: 0.2 + i * 0.1 }}
                  className="bg-white rounded-2xl border border-gray-100 shadow-sm p-5 hover:shadow-md hover:-translate-y-0.5 transition-all duration-200"
                >
                  <div className={`w-10 h-10 ${point.bg} rounded-xl flex items-center justify-center mb-3`}>
                    <point.icon className={`w-5 h-5 ${point.iconColor}`} />
                  </div>
                  <h4 className="font-bold text-gray-900 text-sm mb-1.5">{point.title}</h4>
                  <p className="text-xs text-gray-500 leading-relaxed">{point.desc}</p>
                </motion.div>
              ))}
            </div>

            {/* RPPS badge */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={inView ? { opacity: 1, y: 0 } : {}}
              transition={{ delay: 0.9 }}
              className="mt-6 bg-gradient-to-r from-teal-600 to-teal-700 rounded-2xl p-6 text-white"
            >
              <div className="flex items-center gap-3 mb-3">
                <BadgeCheck className="w-7 h-7 text-teal-200" />
                <p className="font-bold text-lg">RPPS & HCPC Cross-Verified</p>
              </div>
              <p className="text-teal-100 text-sm leading-relaxed">
                PhysioMentor is integrated with the French RPPS national registry and the UK HCPC
                register for automated license cross-checking. Connections to 38 additional European
                regulatory bodies are active.
              </p>
            </motion.div>
          </div>
        </div>
      </div>
    </section>
  )
}
