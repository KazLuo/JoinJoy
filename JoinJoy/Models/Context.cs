using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace JoinJoy.Models
{
    public partial class Context : DbContext
    {
        public Context()
            : base("name=Context")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<Store> Stores { get; set; }

        public virtual DbSet<StorePhoto> StorePhotos { get; set; }
        public virtual DbSet<StoreInventory> StoreInventories { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<MemberCityPref> MemberCityPrefs { get; set; }
        public virtual DbSet<GameType> GameTypes { get; set; }
        public virtual DbSet<MemberGamePref> MemberGamePrefs { get; set; }
        public virtual DbSet<GameDetails> GameDetails { get; set; }
        public virtual DbSet<StoreRating> StoreRatings { get; set; }
        public virtual DbSet<MemberRating> MemberRatings { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupComment> GroupComments { get; set; }
        public virtual DbSet<GroupParticipant> GroupParticipants { get; set; }
        public virtual DbSet<GroupGame> GroupGames { get; set; }
        public virtual DbSet<MemberFollow> MemberFollows { get; set; }
        public virtual DbSet<StoreFollow> StoreFollows { get; set; }


    }
}
