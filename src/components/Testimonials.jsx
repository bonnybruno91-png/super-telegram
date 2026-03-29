import React, { useRef } from 'react'
import { motion } from 'framer-motion'
import { useInView } from 'framer-motion'
import { Star, Quote } from 'lucide-react'

const testimonials = [
  {
    name: 'Léa Fontaine',
    role: 'Physiotherapy Student, Paris',
    initials: 'LF',
    color: 'from-teal-400 to-teal-600',
    rating: 5,
    text:
      "Finding Dr. Moreau on PhysioMentor changed everything for me. In just 3 months, I went from feeling completely lost in clinical placements to presenting confidently at my department. The 1-on-1 focus is unlike anything a classroom can offer.",
  },
  {
    name: 'Marcus Okafor',
    role: 'Final Year Student, London',
    initials: 'MO',
    color: 'from-sky-400 to-sky-600',
    rating: 5,
    text:
      "Prof. Alderton's neurological expertise is extraordinary. He walked me through complex case studies in a way that made everything click. My exam scores have improved dramatically. This platform is a game changer for physio students.",
  },
  {
    name: 'Dr. Anna Pelletier',
    role: 'Mentor & Ortho Specialist, Lyon',
    initials: 'AP',
    color: 'from-violet-400 to-violet-600',
    rating: 5,
    text:
      "Being a mentor on PhysioMentor has been incredibly rewarding. The platform makes scheduling easy, and the students I've connected with are genuinely motivated. Giving back to the profession feels meaningful and structured here.",
  },
]

export default function Testimonials() {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })

  return (
    <section className="py-20 md:py-28 bg-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 24 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
          className="text-center mb-14"
        >
          <span className="inline-block text-xs font-bold text-teal-600 tracking-widest uppercase mb-4 px-3 py-1.5 bg-teal-50 rounded-full border border-teal-100">
            Stories
          </span>
          <h2 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-4">
            What our community says
          </h2>
          <p className="text-lg text-gray-500 max-w-xl mx-auto">
            Real experiences from physiotherapy professionals and students across Europe.
          </p>
        </motion.div>

        <div className="grid md:grid-cols-3 gap-6">
          {testimonials.map((t, i) => (
            <motion.div
              key={t.name}
              initial={{ opacity: 0, y: 30 }}
              animate={inView ? { opacity: 1, y: 0 } : {}}
              transition={{ duration: 0.55, delay: i * 0.13 }}
              className="bg-white rounded-2xl border border-gray-100 shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 p-7 relative"
            >
              {/* Quote icon */}
              <Quote className="absolute top-6 right-6 w-8 h-8 text-gray-100" />

              {/* Stars */}
              <div className="flex gap-0.5 mb-5">
                {[1, 2, 3, 4, 5].map((s) => (
                  <Star key={s} className="w-4 h-4 fill-amber-400 text-amber-400" />
                ))}
              </div>

              {/* Text */}
              <p className="text-sm text-gray-600 leading-relaxed mb-6 italic">"{t.text}"</p>

              {/* Author */}
              <div className="flex items-center gap-3 pt-4 border-t border-gray-100">
                <div className={`w-10 h-10 rounded-full bg-gradient-to-br ${t.color} flex items-center justify-center text-white text-xs font-bold flex-shrink-0`}>
                  {t.initials}
                </div>
                <div>
                  <p className="text-sm font-semibold text-gray-900">{t.name}</p>
                  <p className="text-xs text-gray-400">{t.role}</p>
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
