import React, { useState, useMemo, useRef } from 'react'
import { motion } from 'framer-motion'
import { useInView } from 'framer-motion'
import {
  Search,
  MapPin,
  Star,
  Clock,
  Filter,
  ChevronDown,
  MessageSquare,
  BadgeCheck,
  X,
} from 'lucide-react'

const SPECIALTIES = ['All', 'Sports', 'Neurological', 'Pediatrics', 'Orthopedic', 'Geriatric', 'Cardio']
const LOCATIONS = ['All', 'Paris', 'Lyon', 'London', 'Brussels', 'Zurich', 'Barcelona']

const MENTORS = [
  {
    id: 1,
    name: 'Dr. Rachel Moreau',
    specialty: 'Sports',
    years: 14,
    location: 'Paris, FR',
    rating: 4.9,
    reviews: 87,
    bio: 'Olympic-level sports physio with expertise in ACL recovery and return-to-sport protocols.',
    initials: 'RM',
    color: 'from-teal-400 to-teal-600',
    available: true,
    sessions: 230,
  },
  {
    id: 2,
    name: 'Prof. James Alderton',
    specialty: 'Neurological',
    years: 22,
    location: 'London, UK',
    rating: 5.0,
    reviews: 124,
    bio: 'Specialist in stroke rehabilitation and neuroplasticity-based therapy. Author of 3 clinical guides.',
    initials: 'JA',
    color: 'from-sky-400 to-sky-600',
    available: true,
    sessions: 412,
  },
  {
    id: 3,
    name: 'Dr. Amara Diallo',
    specialty: 'Pediatrics',
    years: 9,
    location: 'Lyon, FR',
    rating: 4.8,
    reviews: 63,
    bio: 'Dedicated to developmental physiotherapy for children. Certified in infant motor assessment.',
    initials: 'AD',
    color: 'from-violet-400 to-violet-600',
    available: false,
    sessions: 155,
  },
  {
    id: 4,
    name: 'Dr. Sofia Hernandez',
    specialty: 'Orthopedic',
    years: 11,
    location: 'Barcelona, ES',
    rating: 4.9,
    reviews: 98,
    bio: 'Post-surgical rehabilitation specialist with expertise in hip and knee replacement recovery.',
    initials: 'SH',
    color: 'from-emerald-400 to-emerald-600',
    available: true,
    sessions: 287,
  },
  {
    id: 5,
    name: 'Dr. Luc Vandermeer',
    specialty: 'Geriatric',
    years: 18,
    location: 'Brussels, BE',
    rating: 4.7,
    reviews: 54,
    bio: 'Expert in falls prevention and mobility programs for elderly patients in residential care.',
    initials: 'LV',
    color: 'from-orange-400 to-orange-600',
    available: true,
    sessions: 193,
  },
  {
    id: 6,
    name: 'Dr. Mia Kaufmann',
    specialty: 'Cardio',
    years: 13,
    location: 'Zurich, CH',
    rating: 4.9,
    reviews: 76,
    bio: 'Cardiac rehab physio helping patients regain strength and confidence after heart events.',
    initials: 'MK',
    color: 'from-rose-400 to-rose-600',
    available: false,
    sessions: 211,
  },
  {
    id: 7,
    name: 'Dr. Thomas Blanc',
    specialty: 'Sports',
    years: 8,
    location: 'Paris, FR',
    rating: 4.8,
    reviews: 41,
    bio: 'Focuses on endurance athletes — cyclists, runners, and triathletes. Former competitive swimmer.',
    initials: 'TB',
    color: 'from-cyan-400 to-cyan-600',
    available: true,
    sessions: 119,
  },
  {
    id: 8,
    name: 'Prof. Nina Osei',
    specialty: 'Neurological',
    years: 27,
    location: 'London, UK',
    rating: 5.0,
    reviews: 201,
      bio: "Internationally recognized in Parkinson's physiotherapy and vestibular rehabilitation.",
    initials: 'NO',
    color: 'from-indigo-400 to-indigo-600',
    available: true,
    sessions: 538,
  },
  {
    id: 9,
    name: 'Dr. Claire Beaumont',
    specialty: 'Pediatrics',
    years: 6,
    location: 'Lyon, FR',
    rating: 4.7,
    reviews: 32,
    bio: 'Passionate about early intervention for children with cerebral palsy and genetic conditions.',
    initials: 'CB',
    color: 'from-pink-400 to-pink-600',
    available: true,
    sessions: 88,
  },
]

function SpecialtyBadge({ specialty }) {
  const colors = {
    Sports: 'bg-teal-50 text-teal-700 border-teal-100',
    Neurological: 'bg-sky-50 text-sky-700 border-sky-100',
    Pediatrics: 'bg-violet-50 text-violet-700 border-violet-100',
    Orthopedic: 'bg-emerald-50 text-emerald-700 border-emerald-100',
    Geriatric: 'bg-orange-50 text-orange-700 border-orange-100',
    Cardio: 'bg-rose-50 text-rose-700 border-rose-100',
  }
  return (
    <span className={`text-xs font-semibold px-2.5 py-1 rounded-full border ${colors[specialty] || 'bg-gray-50 text-gray-600 border-gray-100'}`}>
      {specialty}
    </span>
  )
}

function MentorCard({ mentor, index }) {
  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-60px' })

  return (
    <motion.div
      ref={ref}
      initial={{ opacity: 0, y: 30 }}
      animate={inView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.5, delay: (index % 3) * 0.1, ease: 'easeOut' }}
      className="group bg-white rounded-2xl border border-gray-100 shadow-sm hover:shadow-xl hover:-translate-y-1.5 transition-all duration-300 overflow-hidden"
    >
      {/* Top gradient */}
      <div className={`h-1.5 bg-gradient-to-r ${mentor.color}`} />

      <div className="p-6">
        {/* Header */}
        <div className="flex items-start gap-4 mb-4">
          <div className={`flex-shrink-0 w-14 h-14 rounded-2xl bg-gradient-to-br ${mentor.color} flex items-center justify-center text-white font-bold text-base shadow-md`}>
            {mentor.initials}
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-1.5 mb-0.5">
              <h3 className="font-bold text-gray-900 text-sm truncate">{mentor.name}</h3>
              <BadgeCheck className="w-4 h-4 text-teal-500 flex-shrink-0" />
            </div>
            <SpecialtyBadge specialty={mentor.specialty} />
          </div>
          <div className={`flex-shrink-0 flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${mentor.available ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
            <span className={`w-1.5 h-1.5 rounded-full ${mentor.available ? 'bg-emerald-500' : 'bg-gray-400'}`} />
            {mentor.available ? 'Open' : 'Full'}
          </div>
        </div>

        {/* Bio */}
        <p className="text-xs text-gray-500 leading-relaxed line-clamp-2 mb-4">{mentor.bio}</p>

        {/* Meta */}
        <div className="flex items-center gap-4 text-xs text-gray-500 mb-5">
          <span className="flex items-center gap-1">
            <Clock className="w-3.5 h-3.5" />
            {mentor.years}y exp.
          </span>
          <span className="flex items-center gap-1">
            <MapPin className="w-3.5 h-3.5" />
            {mentor.location}
          </span>
          <span className="flex items-center gap-1">
            <MessageSquare className="w-3.5 h-3.5" />
            {mentor.sessions} sessions
          </span>
        </div>

        {/* Rating */}
        <div className="flex items-center gap-2 mb-5">
          <div className="flex items-center gap-0.5">
            {[1, 2, 3, 4, 5].map((s) => (
              <Star
                key={s}
                className={`w-3.5 h-3.5 ${s <= Math.round(mentor.rating) ? 'fill-amber-400 text-amber-400' : 'text-gray-200'}`}
              />
            ))}
          </div>
          <span className="text-sm font-semibold text-gray-800">{mentor.rating}</span>
          <span className="text-xs text-gray-400">({mentor.reviews} reviews)</span>
        </div>

        {/* CTA */}
        <button
          className={`w-full py-3 rounded-xl text-sm font-semibold transition-all duration-200 hover:-translate-y-0.5 ${
            mentor.available
              ? 'bg-teal-600 hover:bg-teal-700 text-white shadow-sm hover:shadow-md hover:shadow-teal-100'
              : 'bg-gray-100 text-gray-400 cursor-not-allowed'
          }`}
          disabled={!mentor.available}
        >
          {mentor.available ? 'Request Guidance' : 'Join Waitlist'}
        </button>
      </div>
    </motion.div>
  )
}

export default function MentorDirectory() {
  const [search, setSearch] = useState('')
  const [specialty, setSpecialty] = useState('All')
  const [location, setLocation] = useState('All')
  const [showFilters, setShowFilters] = useState(false)

  const ref = useRef(null)
  const inView = useInView(ref, { once: true, margin: '-100px' })

  const filtered = useMemo(() => {
    return MENTORS.filter((m) => {
      const matchSearch =
        !search ||
        m.name.toLowerCase().includes(search.toLowerCase()) ||
        m.specialty.toLowerCase().includes(search.toLowerCase()) ||
        m.location.toLowerCase().includes(search.toLowerCase())
      const matchSpecialty = specialty === 'All' || m.specialty === specialty
      const matchLocation = location === 'All' || m.location.startsWith(location)
      return matchSearch && matchSpecialty && matchLocation
    })
  }, [search, specialty, location])

  const clearFilters = () => {
    setSearch('')
    setSpecialty('All')
    setLocation('All')
  }

  const hasFilters = search || specialty !== 'All' || location !== 'All'

  return (
    <section id="mentors" className="py-20 md:py-28 bg-white">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <motion.div
          ref={ref}
          initial={{ opacity: 0, y: 24 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
          className="text-center mb-12"
        >
          <span className="inline-block text-xs font-bold text-teal-600 tracking-widest uppercase mb-4 px-3 py-1.5 bg-teal-50 rounded-full border border-teal-100">
            Mentor Directory
          </span>
          <h2 className="text-3xl md:text-4xl font-extrabold text-gray-900 mb-4">
            Meet Our Verified Mentors
          </h2>
          <p className="text-lg text-gray-500 max-w-2xl mx-auto">
            Every mentor is license-verified and hand-reviewed by our team. Browse by specialty,
            location, and availability.
          </p>
        </motion.div>

        {/* Search + Filters */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={inView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.55, delay: 0.15 }}
          className="mb-8"
        >
          {/* Search bar */}
          <div className="flex gap-3 mb-4">
            <div className="relative flex-1">
              <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-gray-400" />
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search by name, specialty, or location..."
                className="w-full pl-11 pr-4 py-3.5 bg-gray-50 border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:border-transparent transition-all"
              />
            </div>
            <button
              onClick={() => setShowFilters(!showFilters)}
              className={`flex items-center gap-2 px-5 py-3.5 rounded-xl border text-sm font-medium transition-all ${
                showFilters
                  ? 'bg-teal-600 text-white border-teal-600'
                  : 'bg-white text-gray-700 border-gray-200 hover:border-teal-300 hover:text-teal-600'
              }`}
            >
              <Filter className="w-4 h-4" />
              Filters
              <ChevronDown className={`w-4 h-4 transition-transform ${showFilters ? 'rotate-180' : ''}`} />
            </button>
            {hasFilters && (
              <button
                onClick={clearFilters}
                className="flex items-center gap-1.5 px-4 py-3.5 rounded-xl border border-gray-200 text-sm font-medium text-gray-500 hover:border-red-300 hover:text-red-500 transition-all bg-white"
              >
                <X className="w-4 h-4" />
                Clear
              </button>
            )}
          </div>

          {/* Filter dropdowns */}
          {showFilters && (
            <motion.div
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
              className="grid sm:grid-cols-2 gap-3 p-5 bg-gray-50 rounded-xl border border-gray-200"
            >
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-2 uppercase tracking-wide">
                  Specialty
                </label>
                <div className="flex flex-wrap gap-2">
                  {SPECIALTIES.map((s) => (
                    <button
                      key={s}
                      onClick={() => setSpecialty(s)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                        specialty === s
                          ? 'bg-teal-600 text-white'
                          : 'bg-white text-gray-600 border border-gray-200 hover:border-teal-300'
                      }`}
                    >
                      {s}
                    </button>
                  ))}
                </div>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-2 uppercase tracking-wide">
                  Location
                </label>
                <div className="flex flex-wrap gap-2">
                  {LOCATIONS.map((l) => (
                    <button
                      key={l}
                      onClick={() => setLocation(l)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-all ${
                        location === l
                          ? 'bg-teal-600 text-white'
                          : 'bg-white text-gray-600 border border-gray-200 hover:border-teal-300'
                      }`}
                    >
                      {l}
                    </button>
                  ))}
                </div>
              </div>
            </motion.div>
          )}

          {/* Results count */}
          <p className="text-sm text-gray-400 mt-3">
            Showing{' '}
            <span className="font-semibold text-gray-700">{filtered.length}</span> mentor
            {filtered.length !== 1 ? 's' : ''}
            {hasFilters && ' matching your criteria'}
          </p>
        </motion.div>

        {/* Grid */}
        {filtered.length > 0 ? (
          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5 md:gap-6">
            {filtered.map((mentor, i) => (
              <MentorCard key={mentor.id} mentor={mentor} index={i} />
            ))}
          </div>
        ) : (
          <div className="text-center py-20 text-gray-400">
            <Search className="w-12 h-12 mx-auto mb-4 text-gray-200" />
            <p className="text-lg font-semibold text-gray-500">No mentors found</p>
            <p className="text-sm mt-1">Try adjusting your search or filters</p>
            <button
              onClick={clearFilters}
              className="mt-4 text-teal-600 text-sm font-medium hover:underline"
            >
              Clear all filters
            </button>
          </div>
        )}

        {/* Browse more */}
        <div className="text-center mt-12">
          <a
            href="#"
            className="inline-flex items-center gap-2 px-8 py-4 border-2 border-teal-200 text-teal-700 font-semibold rounded-2xl hover:bg-teal-50 hover:border-teal-300 hover:-translate-y-0.5 transition-all duration-200 text-sm"
          >
            View All {MENTORS.length}+ Mentors
          </a>
        </div>
      </div>
    </section>
  )
}
