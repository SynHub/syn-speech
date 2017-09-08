using java.lang;
using java.util.concurrent;
using java.util.concurrent.atomic;
//PATROLLED
namespace Syn.Speech.Util
{
    public class CustomThreadFactory : ThreadFactory
    {
        private AtomicInteger poolNumber = new AtomicInteger(1);
        readonly ThreadGroup group;
        readonly AtomicInteger threadNumber = new AtomicInteger(1);
        readonly string namePrefix;
        readonly bool daemon;
        readonly int priority;

        public CustomThreadFactory(string namePrefix, bool daemon, int priority)
        {
            if (priority > Thread.MAX_PRIORITY || priority < Thread.MIN_PRIORITY)
                throw new IllegalArgumentException("illegal thread priority");
            SecurityManager s = java.lang.System.getSecurityManager();
            this.group = s != null ? s.getThreadGroup() : Thread.currentThread().getThreadGroup();
            this.namePrefix = namePrefix + "-" + poolNumber.getAndIncrement() + "-thread-";
            this.daemon = daemon;
            this.priority = priority;
        }

        public Thread newThread(Runnable r)
        {
            Thread t = new Thread(group, r, namePrefix + threadNumber.getAndIncrement(), 0);
            if (t.isDaemon() != daemon)
                t.setDaemon(daemon);
            if (t.getPriority() != priority)
                t.setPriority(priority);
            return t;
        }
    }
}
